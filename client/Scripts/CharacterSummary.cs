namespace ProjetoRpg.Client.Scripts;

/// <summary>Only fields returned by the existing character-list/create contract.</summary>
public sealed record CharacterSummary(string CharacterId, string Name, string? ClassId);
