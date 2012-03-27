/*
 * $Id:
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */

using System;
using Mono.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

using NUnit.Framework;

using SolidOpt.Services.Transformations.Multimodel.ILtoCFG;
using SolidOpt.Services.Transformations.CodeModel.ControlFlowGraph;

namespace SolidOpt.Services.Transformations.Multimodel.ILtoCFG.Test
{
	[TestFixture]
	public class CFGTestFixture {
		protected static string testCasesDir
			= Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
							Path.Combine ("..",
							Path.Combine("..", 
			             	Path.Combine ("test", "TestCases" + Path.DirectorySeparatorChar))));
		protected static string testCasesTmpDir = Path.Combine(testCasesDir, "Tmp");
		
		
		
		private static string Normalize(string s)
		{
			char[] CharsToTrim = new char[]{' ','\t'};
			// for Mac, Win, Lin
			return s.Normalize().Replace("\n\r", "\n").Replace("\r\n", "\n").Replace("\r", "\n").Trim(CharsToTrim);
		}
		
		public void RunTestCase(string testCaseName)
		{
			MethodDefinition mDef = LoadTestCaseMethod(testCaseName);
			CilToControlFlowGraph TransforIL = new CilToControlFlowGraph();
			ControlFlowGraph CFG = TransforIL.Process(mDef.Body);
			Assert.IsTrue(Validate(CFG, testCaseName), "Different");
		}
		
		public bool Validate(ControlFlowGraph graph, string testCaseName)
		{
			string cfg = DumpBasicBlock(graph.Root);
			string resultFile = GetTestCaseFullPath(testCaseName) + ".il.cfg";

			if (!File.Exists(resultFile))
				return false;

			string seen = Normalize(cfg);
			string expected = Normalize(File.ReadAllText(resultFile));
			return seen == expected;
		}
		
		public string DumpBasicBlock(BasicBlock block)
		{
			StringBuilder sb = new StringBuilder();
			
			sb.AppendLine(String.Format("block {0}:", block.Name));
			sb.AppendLine("\tbody:");
			foreach (Instruction instruction in block) 
				sb.AppendLine(String.Format("\t\t{0}", instruction.ToString()));
			
			if (block.Successors != null && block.Successors.Count > 0) {
				sb.AppendLine("\tsuccessors:");
				foreach (BasicBlock succ in block.Successors) {
					sb.AppendLine(String.Format("\t\tblock {0}", succ.Name));
				}
			}
			
			if (block.Predecessors != null && block.Predecessors.Count > 0) {
				sb.AppendLine("\tpredecessor:");
				foreach (BasicBlock pred in block.Predecessors) {
					sb.AppendLine(String.Format("\t\tblock {0}", pred.Name));
				}
			}
			
			foreach(BasicBlock succ in block.Successors)
				sb.AppendLine(DumpBasicBlock(succ));
			
			return sb.ToString();
		}
		
		private string GetTestCaseFullPath(string testCaseName)
		{
			return Path.Combine(testCasesDir, testCaseName);
		}

		private string CompileTestCase (string testCaseName)
		{
			string sourceFile = GetTestCaseFullPath(testCaseName) + ".il";
			Assert.IsTrue(File.Exists (sourceFile), sourceFile + " not found!");
			string testCaseAssemblyName = Path.Combine(testCasesTmpDir, testCaseName+".dll");
			ilasm (string.Format ("/DLL \"/OUTPUT:{0}\" {1}", testCaseAssemblyName, sourceFile));
			
			return testCaseAssemblyName;
		}
		
		static void ilasm (string arguments)
		{
			Process p = new Process();
			p.StartInfo.Arguments = arguments;
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardInput = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.FileName = "ilasm";
			p.Start();
			string output = p.StandardOutput.ReadToEnd ();
			string error = p.StandardError.ReadToEnd ();
			p.WaitForExit();
			Assert.AreEqual (0, p.ExitCode, output + error);
		}

		protected MethodDefinition LoadTestCaseMethod(string testCaseName)
		{
			string testCaseAssemblyName = CompileTestCase(testCaseName);

			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(testCaseAssemblyName);
			TypeDefinition type = assembly.MainModule.GetType("TestCase");
			Assert.IsNotNull (type, "Type TestCase not found!");
			MethodDefinition found = GetMethod(type.Methods, "Main");
			Assert.IsNotNull (found, "Method TestCase.Main not found!");
			return found;
		}	
		
		MethodDefinition GetMethod(Collection<MethodDefinition> methods, string name)
		{
			foreach (MethodDefinition mDef in methods) {
				if (mDef.Name == name)
					return mDef;
			}
			return null;
		}
		
		[Test]
		public void Empty() 
		{
			RunTestCase("Empty");
		}
		
		[Test]
		public void SimpleIf() 
		{
			RunTestCase("SimpleIf");
		}

	}
}
