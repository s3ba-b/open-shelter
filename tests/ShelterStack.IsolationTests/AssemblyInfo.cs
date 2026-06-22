using Xunit;

// Each isolation test class points its real host(s) at a Testcontainers Postgres by setting
// process-global ConnectionStrings__* environment variables in InitializeAsync. xUnit runs
// separate test classes in parallel by default, which would let one class's connection string
// clobber another's between a host booting and its lazy first-request seeding — so serialize
// the whole assembly. The hosts each take a few seconds to spin up a container anyway, so the
// lost parallelism costs little.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
