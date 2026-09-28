using Xunit;

// O numerador é estado de processo compartilhado entre as classes de teste:
// rodar classes em paralelo faria uma enxergar o contador da outra.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
