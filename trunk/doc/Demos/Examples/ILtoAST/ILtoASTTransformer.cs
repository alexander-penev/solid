﻿/*
 * Created by SharpDevelop.
 * User: Vassil Vassilev
 * Date: 24.6.2009 г.
 * Time: 17:23
 * 
 */
 
using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Cecil.Decompiler;
using Cecil.Decompiler.Cil;
using Cecil.Decompiler.Languages;

using AstMethodDefinitionModel;

using Cecil.Decompiler.Steps;

using SolidOpt.Optimizer.Transformers;

namespace ILtoAST
{
	/// <summary>
	/// Description of NopRemoveTransformer.
	/// </summary>
	public class ILtoASTTransformer : ITransform<MethodDefinition, AstMethodDefinition>
	{
		public ILtoASTTransformer()
		{
		}

		public AstMethodDefinition Transform(MethodDefinition source)
		{
			ILanguage csharpLang = CSharp.GetLanguage(CSharpVersion.V1);//new CSharp();
//			DecompilationPipeline pipeline = csharpLang.CreatePipeline();
			DecompilationPipeline pipeline = new DecompilationPipeline (
				new StatementDecompiler (BlockOptimization.Detailed),
				RemoveLastReturn.Instance,
//				PropertyStep.Instance,
//				CanCastStep.Instance,
//				RebuildForStatements.Instance,
//				RebuildForeachStatements.Instance,
//				DeclareVariablesOnFirstAssignment.Instance,
//				DeclareTopLevelVariables.Instance,
				DeclareVariables.Instance
//				SelfAssignement.Instance,
//				OperatorStep.Instance
			);
			pipeline.Run(source.Body);
			
			
			
			var cfg = ControlFlowGraph.Create (source);

			FormatControlFlowGraph (Console.Out, cfg);

//			Console.WriteLine ("--------------------");

//			var store = AnnotationStore.CreateStore (cfg, BlockOptimization.Detailed);
//			PrintAnnotations (method, store);

//			var language = CSharp.GetLanguage (CSharpVersion.V1);
			

			//var body = method.Body.Decompile (language);

			var writer = csharpLang.GetWriter (new PlainTextFormatter (Console.Out));

			writer.Write (source);
			
			
			
			return new AstMethodDefinition(source, pipeline.Body);
		}
		
		public static void FormatControlFlowGraph (System.IO.TextWriter writer, ControlFlowGraph cfg)
		{
			foreach (InstructionBlock block in cfg.Blocks) {
				writer.WriteLine ("block {0}:", block.Index);
				writer.WriteLine ("\tbody:");
				foreach (Instruction instruction in block) {
					writer.Write ("\t\t");
					var data = cfg.GetData (instruction);
					writer.Write ("[{0}:{1}] ", data.StackBefore, data.StackAfter);
					Formatter.WriteInstruction (writer, instruction);
					writer.WriteLine ();
				}
				InstructionBlock [] successors = block.Successors;
				if (successors.Length > 0) {
					writer.WriteLine ("\tsuccessors:");
					foreach (InstructionBlock successor in successors) {
						writer.WriteLine ("\t\tblock {0}", successor.Index);
					}
				}
			}
		}
	}
}
