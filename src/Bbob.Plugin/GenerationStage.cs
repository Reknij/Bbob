namespace Bbob.Plugin;

/// <summary>
/// Stage of generation.
/// </summary>
public enum GenerationStage
{
    /// <summary>'Initialize' stage. General provide data in this stage.</summary>
    Initialize,
    /// <summary>'Process' stage. General process data in this stage.</summary>
    Process,
    /// <summary>'Parse' stage. General parsing data in this stage.</summary>
    Parse,
    /// <summary>'FinalProcess' stage. General process data again in this stage.</summary>
    FinalProcess,
    /// <summary>'Confirm' stage. General get the data in this stage. It convention not modify in this stage.</summary>
    Confirm
}