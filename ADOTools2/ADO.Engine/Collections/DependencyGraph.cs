using System.Collections.Generic;
using System.Linq;

namespace ADO.Collections
{
    public sealed class DependencyGraph<T>
    {
        #region - Private Members

        private readonly HashSet<T> _nodes = new HashSet<T>();
        private readonly HashSet<T> _independentNodes = new HashSet<T>();
        private readonly Dictionary<T, HashSet<T>> _dependenciesByNode = new Dictionary<T, HashSet<T>>();

        #endregion

        #region - Public Members

        public IEnumerable<T> DependentNodes
        {
            get
            {
                return _nodes;
            }
        }

        public IEnumerable<T> IndependentNodes
        {
            get
            {
                return _independentNodes;
            }
        }

        public void Add(T independent)
        {
            // Add independent.
            _independentNodes.Add(independent);
        }

        public void AddDependency(T dependent, T dependency)
        {
            // Add dependent and dependencies.
            _nodes.Add(dependent);
            _nodes.Add(dependency);

            // get the list of dependencies for this dependent (create if it doesn't exist yet)            
            if (!_dependenciesByNode.TryGetValue(dependent, out HashSet<T> dependencySet))
                dependencySet = _dependenciesByNode[dependent] = new HashSet<T>();

            // Add to dependency set.
            dependencySet.Add(dependency);
        }

        public IEnumerable<T> GetDependenciesForNode(T dependant)
        {
            if (_dependenciesByNode.TryGetValue(dependant, out HashSet<T> dependencies))
                return dependencies;
            else
                return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Remove edges from the graph without changing the reachability of nodes.
        /// If a node has both a direct and transitive dependency upon another node, the direct dependency is removed.
        /// </summary>
        public void TransitiveReduce()
        {
            HashSet<T> visited = new HashSet<T>();
            Stack<T> frontier = new Stack<T>();

            foreach (T root in _nodes)
            {
                if (!_dependenciesByNode.TryGetValue(root, out HashSet<T>  children))
                    continue;

                // Extract the list of child nodes.
                List<T> childList = children.ToList();

                for (int i = 0; i < childList.Count; i++)
                {
                    // Get child node.
                    T child = childList[i];

                    // Clear set.
                    visited.Clear();
                    // Add the root to visited set.
                    visited.Add(root);

                    // Perform a depth-first search from this child.
                    frontier.Push(child);

                    while (frontier.Any())
                    {
                        // Get node.
                        T node = frontier.Pop();

                        // If we've already expanded this node, continue...
                        if (!visited.Add(node))
                            continue;

                        foreach (T dependency in GetDependenciesForNode(node))
                            frontier.Push(dependency);

                        if (!Equals(child, node) && children.Contains(node))
                        {
                            // This node is directly linked to from root,
                            // however we arrived here via a longer, indirect path
                            // so we remove the shorter, direct path
                            children.Remove(node);

                            // Update the list we're iterating over, adjusting the current index if required
                            int index = childList.IndexOf(node);

                            // Remote child node.
                            childList.RemoveAt(index);

                            // Decrement counter if...
                            if (index <= i)
                                i--;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
