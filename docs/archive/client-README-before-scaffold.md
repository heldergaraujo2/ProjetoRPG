# Cliente Godot — estrutura alvo

Reservado ao projeto Godot 4 com suporte .NET/C# aprovado em [ADR-001](../docs/adr/ADR-001.md). A pasta não contém projeto compilável nesta revisão: Godot/Godot .NET SDK e `dotnet` não estão instalados no ambiente auditado. Inicializar os arquivos `project.godot`, `.csproj`, cenas e testes somente com toolchain compatível disponível; não declarar build até executá-lo.
