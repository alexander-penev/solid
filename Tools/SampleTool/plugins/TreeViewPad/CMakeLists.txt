csharp_add_library(
  TreeViewPad.dll
  ${CMAKE_CURRENT_SOURCE_DIR}/*.cs
  ${CMAKE_CURRENT_BINARY_DIR}/AssemblyInfo.cs
  )
csharp_add_dependencies(
  TreeViewPad.dll
  ${CMAKE_RUNTIME_OUTPUT_DIR}/SampleTool.exe
  # External
  ${CMAKE_LIBRARY_OUTPUT_DIR}/SolidOpt.Services.dll
  ${CMAKE_LIBRARY_OUTPUT_DIR}/SolidV.dll
  # GAC
  pkg:gtk-sharp-2.0
  )
