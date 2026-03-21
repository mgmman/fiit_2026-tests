using NUnit.Framework;

// LevelOfParallelism из дз уже выстовлен)
[assembly: LevelOfParallelism(5)]
[assembly: Parallelizable(ParallelScope.Fixtures)]