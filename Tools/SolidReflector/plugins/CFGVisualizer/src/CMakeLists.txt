##
## $Id$
## It is part of the SolidOpt Copyright Policy (see Copyright.txt)
## For further details see the nearest License.txt
##

csharp_add_library(
  CFGVisualizer.dll
  ${CMAKE_CURRENT_SOURCE_DIR}/*.cs
  ${CMAKE_CURRENT_BINARY_DIR}/AssemblyInfo.cs
  )
csharp_add_dependencies(
  CFGVisualizer.dll
  ${CMAKE_RUNTIME_OUTPUT_DIR}/SolidReflector.exe
  ${CMAKE_LIBRARY_OUTPUT_DIR}/AssemblyBrowser.dll
  # External
  ${CMAKE_LIBRARY_OUTPUT_DIR}/SolidOpt.Services.dll
  ${CMAKE_LIBRARY_OUTPUT_DIR}/SolidOpt.Services.Transformations.dll
  ${CMAKE_LIBRARY_OUTPUT_DIR}/SolidOpt.Services.Transformations.CodeModel.ControlFlowGraph.dll
  ${CMAKE_LIBRARY_OUTPUT_DIR}/SolidOpt.Services.Transformations.MultiModel.dll
  ${CMAKE_LIBRARY_OUTPUT_DIR}/SolidOpt.Services.Transformations.MultiModel.ILtoCFG.dll
  ${CMAKE_LIBRARY_OUTPUT_DIR}/SolidOpt.Services.Transformations.MultiModel.CFGtoIL.dll
  ${CMAKE_LIBRARY_OUTPUT_DIR}/Mono.Cecil.dll
  ${CMAKE_LIBRARY_OUTPUT_DIR}/SolidV.dll
  # GAC
  Mono.Cairo.dll
  # Packages
  "pkg:gtk-sharp-2.0"
  )
