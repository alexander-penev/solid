using Gtk;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoDevelop.Components.Docking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Runtime.Remoting;

using SolidOpt.Services;
using SolidReflector.Plugins.AssemblyBrowser;
using SolidOpt.Services.Transformations.CodeModel.ControlFlowGraph;
using SolidOpt.Services.Transformations.Multimodel.ILtoCFG;
using SolidOpt.Services.Transformations.Multimodel.CFGtoIL;

namespace SolidReflector.Plugins.CFGVisualizer
{
  public class CFGVisualizer : IPlugin
  {
    private MainWindow mainWindow = null;
    private DrawingArea drawingArea = null;
    private DockItem cfgVisualizingDock = null;
    private DockItem simulationDock = null;
    private ControlFlowGraph currentCfg = null;

    public CFGVisualizer() { }

    #region IPlugin implementation
    void IPlugin.Init(object context)
    {
      ISolidReflector reflector = context as ISolidReflector;
      mainWindow = reflector.GetMainWindow();
      IAssemblyBrowser browser = reflector.GetPlugins().GetService<IAssemblyBrowser>();
      browser.SelectionChanged += OnSelectionChanged;

      drawingArea = new DrawingArea();

      ScrolledWindow scrollWindow = new ScrolledWindow();
      Viewport viewport = new Viewport();

      scrollWindow.Add(viewport);
      viewport.Add(drawingArea);
      
      Gtk.Notebook nb = new Gtk.Notebook();
      nb.AppendPage(new TextView(), new Gtk.Label("CFG Text"));
      nb.AppendPage(scrollWindow, new Gtk.Label("CFG Visualizer"));
      nb.ShowAll();

      cfgVisualizingDock = mainWindow.DockFrame.AddItem("CFG Visualizer");
      cfgVisualizingDock.Expand = true;
      cfgVisualizingDock.DrawFrame = true;
      cfgVisualizingDock.Label = "CFG Visualizer";
      cfgVisualizingDock.Content = nb;
      cfgVisualizingDock.DefaultVisible = true;
      cfgVisualizingDock.Visible = true;

      VBox vBox = new VBox(false, 0);
      TextView textView = new TextView();
      TextView outputView = new TextView();
      Button simulateButton = new Button("Simulate");
      simulateButton.Clicked += HandleClicked;

      vBox.PackStart(simulateButton, false, false, 0);
      vBox.PackStart(new Label("New CFG: "), false, false, 0);
      vBox.PackStart(textView, true, true, 0);
      vBox.PackStart(new Label("Method output: "), false, false, 0);
      vBox.PackEnd(outputView, true, true, 0);
      
      vBox.ShowAll();

      simulationDock = mainWindow.DockFrame.AddItem("Simulation Visualizer");
      simulationDock.Expand = true;
      simulationDock.DrawFrame = true;
      simulationDock.Label = "Simulation Visualizer";
      simulationDock.Content = vBox;
      simulationDock.DefaultVisible = true;
      simulationDock.Visible = true;
    }

    void IPlugin.UnInit(object context)
    {
      cfgVisualizingDock.Visible = false;
      // BUG: Object not set to an instance of an object exception if only one plugin is loaded
      // and attempted to be UnInit-ed
      mainWindow.DockFrame.RemoveItem(cfgVisualizingDock);
    }
    #endregion

    private void OnSelectionChanged(object sender, SelectionEventArgs args) {
      Gtk.Notebook nb = cfgVisualizingDock.Content as Gtk.Notebook;
      Gtk.TextView textView = nb.GetNthPage(0) as Gtk.TextView;
      //Gtk.DrawingArea drawingArea = nb.GetNthPage(1) as Gtk.DrawingArea;

      if (args.definition != null) {
        // Dump the definition
        CFGPrettyPrinter.PrintPretty(args.definition, textView);
        CFGPrettyDrawer drawer = new CFGPrettyDrawer(drawingArea);

        if (args.definition is MethodDefinition) {
          var builder = new ControlFlowGraphBuilder(args.definition as MethodDefinition);
          currentCfg = builder.Create();

          drawer.DrawTextBlocks(currentCfg);
          if (args.module != null) {
            // Dump the module
            if (args.assembly != null) {
              // Dump assembly modules.
            }
          }
        }
      }
    }

    void HandleClicked(object sender, EventArgs e)
    {
      if (currentCfg != null) {
        createAssemblyFromCfg(currentCfg);
        CFGPrettyDrawer drawer = new CFGPrettyDrawer(drawingArea);
        drawer.DrawTextBlocks(currentCfg);

        TextView textView = (simulationDock.Content as Gtk.VBox).Children[2] as TextView;
        textView.Buffer.Clear();
        textView.Buffer.Text = currentCfg.ToString();
        //CFGPrettyPrinter.PrintPretty(mDef, textView);
      }
    }

    /// <summary>
    /// Dynamically creates an assembly from a control flow graph.
    /// </summary>
    /// <param name="cfg">Control flow graph</param>
    /// 
    private void createAssemblyFromCfg(ControlFlowGraph cfg) {
      ControlFlowGraphToCil cfgTransformer = new ControlFlowGraphToCil();
      MethodDefinition methodDef = cfgTransformer.Transform(currentCfg);

      AssemblyNameDefinition assemblyName = new AssemblyNameDefinition("Program", new Version("1.0.0.0"));
      AssemblyDefinition assemblyDef = AssemblyDefinition.CreateAssembly(assemblyName, "<Module>",
                                                                    ModuleKind.Console);

      TypeReference objTypeRef = assemblyDef.MainModule.Import(typeof(System.Object));
      TypeDefinition typeDef = new TypeDefinition("Simulation", "MainClass", TypeAttributes.Public,
                                                  objTypeRef);
      assemblyDef.Modules[0].Types.Add(typeDef);

      // Create the main method: public static void Main ()
      TypeReference voidTypeRef = assemblyDef.MainModule.Import(typeof(void));
      MethodDefinition mainMethodDef = new MethodDefinition("Main", MethodAttributes.Public |
                                                                    MethodAttributes.HideBySig |
                                                                    MethodAttributes.Static,
                                                            voidTypeRef);

      // Cannot use methodDef due to it is already in use
      // so we have to create identical MethodDefinition that would be able to be called
      MethodDefinition trampolineMethod = new MethodDefinition(methodDef.Name, methodDef.Attributes,
                                                               methodDef.ReturnType);

      // Add the parameters of the callable method
      foreach(ParameterDefinition pDef in methodDef.Parameters)
        trampolineMethod.Parameters.Add(new ParameterDefinition(pDef.Name, pDef.Attributes, 
                                                                pDef.ParameterType));

      // Add the instructions to the callable method
      ILProcessor trampolineCIL = trampolineMethod.Body.GetILProcessor();
      foreach (Instruction instr in methodDef.Body.Instructions) {
        if (instr.OpCode == OpCodes.Call)
          instr.Operand = assemblyDef.MainModule.Import(instr.Operand as MethodReference);

        trampolineCIL.Append(instr);
      }

      typeDef.Methods.Add(trampolineMethod);

      MethodReference importedMRef = assemblyDef.MainModule.Import(methodDef);

      // Add just ret to the Main method
      ILProcessor cil = mainMethodDef.Body.GetILProcessor();
      cil.Append(cil.Create(OpCodes.Ret));

      typeDef.Methods.Add(mainMethodDef);
      assemblyDef.EntryPoint = mainMethodDef;

      string assemblyPath = Path.Combine(Path.GetTempPath(), "temp.exe");
      assemblyDef.Write(assemblyPath);

      Sandbox sandbox = new Sandbox(assemblyPath, trampolineMethod.Name);
      TextView textView = (simulationDock.Content as Gtk.VBox).Children[4] as TextView;
      textView.Buffer.Clear();
      textView.Buffer.Text = sandbox.SimulationMethodOutput;
    }
  }

  /// <summary>
  /// Loads assembly in separate application domain with restricted permissions.
  /// Currently mono does not support CAS.
  /// </summary>
  /// 
  public class Sandbox : MarshalByRefObject
  {
    public string SimulationMethodOutput = null;
    public Sandbox() { }
    public Sandbox(string assemblyPath, string method) {

      // Gives the loaded assembly the ability to be executed
      PermissionSet permSet = new PermissionSet(PermissionState.None);
      permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
      
      AppDomainSetup adSetup = new AppDomainSetup();
      adSetup.ApplicationName = "AppDomainSetup";
      AppDomain domain = AppDomain.CreateDomain("SandboxDomain");
      ObjectHandle handle = Activator.CreateInstanceFrom(domain, 
                                       typeof(Sandbox).Assembly.ManifestModule.FullyQualifiedName,
                                       typeof(Sandbox).FullName);
      
      Sandbox sandboxInstance = (Sandbox) handle.Unwrap();
      SimulationMethodOutput = sandboxInstance.ExecuteAssembly(assemblyPath, "Simulation.MainClass", method);
    }

    /// <summary>
    /// Executes an assembly in a different and permission restricted app domain.
    /// </summary>
    /// <param name="assemblyPath">Assembly path.</param>
    /// <param name="typeName">Type name.</param>
    /// <param name="method">Method.</param>
    /// 
    public string ExecuteAssembly(string assemblyPath, string typeName, string method) {
      System.Reflection.Assembly a = System.Reflection.Assembly.LoadFrom(assemblyPath);
      System.Reflection.Module module = a.GetModule("<Module>");
      Type namespaceType = module.GetType(typeName);
      System.Reflection.MethodInfo methodToExecute = namespaceType.GetMethod(method);
      System.Reflection.ParameterInfo[] parameters = methodToExecute.GetParameters();

      Random r = new Random();
      List<object> sampleArgs = new List<object>();
      long sample = 0;

      foreach (System.Reflection.ParameterInfo param in parameters) {
        switch (Type.GetTypeCode(param.ParameterType)) {
          case TypeCode.Int16:
            sampleArgs.Add(r.Next(Int16.MinValue, Int16.MaxValue));
            break;
          case TypeCode.Int32:
          case TypeCode.Int64:
            sampleArgs.Add(r.Next(Int32.MinValue, Int32.MaxValue));
            break;
          default:
            System.Diagnostics.Debug.Assert(false, "Not supported: " + param.ParameterType.ToString());
            break;
        }
      }

      return methodToExecute.Invoke(null, sampleArgs.ToArray()).ToString();
    }
  }
}
