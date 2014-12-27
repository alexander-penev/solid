﻿/*
 * $Id$
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */

using System;
using System.Text;

using Mono.Cecil;

using Cecil.Decompiler.Ast;

namespace SolidOpt.Services.Transformations.CodeModel.AbstractSyntacticTree
{
  /// <summary>
  /// Description of AstMethodDefinition.
  /// </summary>
  public class AstMethodDefinition
  {
    
    private MethodDefinition method;    
    public MethodDefinition Method {
      get { return method; }
      set { method = value; }
    }
    
    private BlockStatement block;
    public BlockStatement Block {
      get { return block; }
      set { block = value; }
    }
    
    public AstMethodDefinition()
    {
    }
    
    public AstMethodDefinition(MethodDefinition method, BlockStatement block)
    {
      this.method = method;
      this.block = block;
    }

    public override string ToString()
    {
      CodeVisitor codeVisitor = new CodeVisitor();
      codeVisitor.Visit(block);
      return codeVisitor.Text;
    }
            
    internal class CodeVisitor : Cecil.Decompiler.Ast.BaseCodeVisitor {
      private StringBuilder text = new StringBuilder();
      public string Text {
        get { return text.ToString(); }
      }
      private int indent = 0;

      public override void Visit (ICodeNode node)
      {
        if (null == node)
          return;
        for(int i = 0; i <= indent; i++)
          if (i + 1 > indent)
            text.Append("+--");
          else
            text.Append("---");

        text.Append(node.CodeNodeType.ToString());

        if (node is MethodReferenceExpression)
          text.Append(" " + (node as MethodReferenceExpression).Method.ToString());
        else if (node is VariableDeclarationExpression)
          text.AppendFormat(" ({0})", (node as VariableDeclarationExpression).Variable.ToString());
        else if (node is VariableReferenceExpression)
          text.AppendFormat(" ({0})", (node as VariableReferenceExpression).Variable.ToString());
        else if (node is LiteralExpression)
          text.AppendFormat(" ({0})", (node as LiteralExpression).Value.ToString());

        text.AppendLine();
          
        indent++;
        base.Visit(node);
        indent--;
        }
      }
  }
}