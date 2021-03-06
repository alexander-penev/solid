/*
 * $Id$
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */
using System;

namespace DataMorphose.Actions
{
  public interface IReversibleAction
  {
    void Undo();
    void Redo();
  }
}

