using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ADO.Collections;

namespace ADO.Engine.Tests
{
    [TestClass]
    public sealed class DependencyGraphTest
    {
        [TestMethod]
        public void TestSortDependencyGraph()
        {
            DependencyGraph<string> graph = new DependencyGraph<string>();

            // Using the example with edges of the graph
            // here: https://en.wikipedia.org/wiki/Topological_sorting#Examples
            // 7 depends on 11
            // 7 depends on 8
            // 5 depends on 11
            // 3 depends on 8
            // ...
            graph.Add("1");
            graph.Add("12");
            graph.AddDependency("7", "11");
            graph.AddDependency("7", "8");
            graph.AddDependency("5", "11");
            graph.AddDependency("3", "8");
            graph.AddDependency("3", "10");
            graph.AddDependency("11", "2");
            graph.AddDependency("11", "9");
            graph.AddDependency("11", "10");
            graph.AddDependency("8", "9");
            graph.TransitiveReduce();

            // Perform a topological sort and reverse topological sort.
            List<string> tsl = Utility.GetSortedDependencyGraphNodes(graph);
            List<string> rtsl = Utility.GetSortedDependencyGraphNodes(graph, true);

            // This list should be in order from the most dependent to less dependent.
            CollectionAssert.AreEqual(new[] { "3", "5", "7", "11", "2", "8", "9", "10", "1", "12" }, tsl);
            
            // This list should be in reverse order from the less dependent to most dependent.
            CollectionAssert.AreEqual(new[] { "1", "12", "10", "9", "8", "2", "11", "7", "5", "3" }, rtsl);
        }

        [TestMethod]
        public void AddDependencyTest()
        {
            DependencyGraph<string> graph = new DependencyGraph<string>();
            graph.AddDependency("A", "B");
            graph.AddDependency("B", "C");

            CollectionAssert.AreEqual(new[] { "A", "B", "C" }, graph.DependentNodes.ToList());
            CollectionAssert.AreEqual(new[] { "B" }, graph.GetDependenciesForNode("A").ToList());
        }

        [TestMethod]
        public void TransitiveReduceThreeNodes()
        {
            DependencyGraph<char> graph = new DependencyGraph<char>();
            graph.AddDependency('a', 'b');
            graph.AddDependency('b', 'c');
            graph.AddDependency('a', 'c');

            graph.TransitiveReduce();

            CollectionAssert.AreEqual(new[] { 'a', 'b', 'c' }, graph.DependentNodes.ToList());
            CollectionAssert.AreEqual(new[] { 'b' }, graph.GetDependenciesForNode('a').ToList());
            CollectionAssert.AreEqual(new[] { 'c' }, graph.GetDependenciesForNode('b').ToList());
        }

        [TestMethod]
        public void TransitiveReduceCascade()
        {
            DependencyGraph<char> graph = new DependencyGraph<char>();

            for (var c1 = 'a'; c1 < 'f'; c1++)
                for (var c2 = (char)(c1 + 1); c2 < 'f'; c2++)
                    graph.AddDependency(c1, c2);

            graph.TransitiveReduce();

            for (var c = 'a'; c < 'f' - 1; c++)
                CollectionAssert.AreEqual(new[] { (char)(c + 1) }, graph.GetDependenciesForNode(c).ToList(), $"Dep of {c} should be {(char)(c + 1)}");
        }

        [TestMethod]
        public void TransitiveReduceCycles()
        {
            DependencyGraph<char> graph = new DependencyGraph<char>();

            graph.AddDependency('a', 'b'); // a <--> b
            graph.AddDependency('b', 'a');

            graph.AddDependency('a', 'c'); // a --> c
            graph.AddDependency('b', 'c'); // b --> c

            graph.TransitiveReduce();

            // NOTE result here depends upon order of enumeration from HashSet<char>, making this test potentially fragile

            CollectionAssert.AreEqual(new[] { 'b' }, graph.GetDependenciesForNode('a').ToList());
            CollectionAssert.AreEqual(new[] { 'a', 'c' }, graph.GetDependenciesForNode('b').ToList());
        }
    }
}
