using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TreeCollections;

namespace ADO.Engine.BusinessEntities
{
    public partial class SimpleMutableClassificationNodeMinimalWithIdNode
    {
        public void SaveToJson(string path)
        {
            ClassificationNodeMinimalWithIdDataNode dataRoot = ToClassificationNodeMinimalWithIdDataNodeRecursive();
            string json = JsonConvert.SerializeObject(dataRoot, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        public static SimpleMutableClassificationNodeMinimalWithIdNode LoadFromJson(string areaInitializationTreePath)
        {
            string json = File.ReadAllText(areaInitializationTreePath);
            ClassificationNodeMinimalWithIdDataNode dataRoot
                = JsonConvert.DeserializeObject<ClassificationNodeMinimalWithIdDataNode>(json);

            var root = new SimpleMutableClassificationNodeMinimalWithIdNode(
                new ClassificationNodeMinimalWithIdItem(
                    dataRoot.Id,
                    dataRoot.Name,
                    dataRoot.StructureType,
                    dataRoot.HasChildren,
                    dataRoot.Path,
                    dataRoot.Attributes
                    ));
            root.Build(dataRoot, n =>
            new ClassificationNodeMinimalWithIdItem(
                n.Id,
                n.Name,
                n.StructureType,
                n.HasChildren,
                n.Path,
                n.Attributes
                ));
            return root;
        }

        public ClassificationNodeMinimal ToClassificationNodeMinimal()
        {
            ClassificationNodeMinimalWithIdDataNode value = this.ToClassificationNodeMinimalWithIdDataNodeRecursive();
            //quick and dirty
            string json = JsonConvert.SerializeObject(value, Formatting.Indented);
            //essentially stripping id's
            ClassificationNodeMinimal classificationNodeMinimal = JsonConvert.DeserializeObject<ClassificationNodeMinimal>(json);
            return classificationNodeMinimal;
        }

        public ClassificationNodeMinimalWithIdDataNode ToClassificationNodeMinimalWithIdDataNodeRecursive()
        {
            if (this.Children.Count == 0)
            {
                ClassificationNodeMinimalWithIdDataNode newClassificationNodeMinimalWithIdDataNodeRoot =
                    new ClassificationNodeMinimalWithIdDataNode()
                    {
                        Id = this.Item.Id,
                        Name = this.Item.Name,
                        HasChildren = false,
                        Children = new ClassificationNodeMinimalWithIdDataNode[] { },
                        Attributes = this.Item.Attributes,
                        Path = this.Item.Path,
                        StructureType = this.Item.StructureType
                    };
                return newClassificationNodeMinimalWithIdDataNodeRoot;
            }
            else
            {
                ClassificationNodeMinimalWithIdDataNode newClassificationNodeMinimalWithIdDataNodeRoot = new ClassificationNodeMinimalWithIdDataNode()
                {
                    Id = this.Item.Id,
                    Name = this.Item.Name,
                    HasChildren = true,
                    Children = new ClassificationNodeMinimalWithIdDataNode[this.Children.Count],
                    Attributes = this.Item.Attributes,
                    Path = this.Item.Path,
                    StructureType = this.Item.StructureType
                };
                int childCount = 0;
                foreach (var child in this.Children)
                {
                    newClassificationNodeMinimalWithIdDataNodeRoot.Children[childCount++] = child.ToClassificationNodeMinimalWithIdDataNodeRecursive();
                }
                return newClassificationNodeMinimalWithIdDataNodeRoot;
            }
        }

        public SimpleMutableClassificationNodeMinimalWithIdNode AddTree(
            //string portfolio, string programOrProduct,
            List<string> prefixedNodeNames,
            //string project,
            SimpleMutableClassificationNodeMinimalWithIdNode simpleMutableClassificationNodeMinimalWithIdNode)
        {
            //SimpleMutableClassificationNodeMinimalWithIdNode initialRoot = this;
            SimpleMutableClassificationNodeMinimalWithIdNode currentParentNode = this;
            if (prefixedNodeNames != null && prefixedNodeNames.Count > 0)
            {
                foreach (var prefixedNodeName in prefixedNodeNames)
                {
                    currentParentNode
                        = AddChildIfItDoesNotExist(currentParentNode, prefixedNodeName);
                }
                //SimpleMutableClassificationNodeMinimalWithIdNode portfolioNode
                //    = AddChildIfItDoesNotExist(this, portfolio);
                //SimpleMutableClassificationNodeMinimalWithIdNode programOrProductNode
                //    = AddChildIfItDoesNotExist(portfolioNode, programOrProduct);
            }
            else
            {
                //throw new NotImplementedException();
            }
            //if (string.IsNullOrEmpty(project))
            //{
            //    SimpleMutableClassificationNodeMinimalWithIdNode retRoot =
            //    AddTreeRecursive(currentParentNode, simpleMutableClassificationNodeMinimalWithIdNode);
            //    return retRoot;
            //}
            //else
            //{
            //    SimpleMutableClassificationNodeMinimalWithIdNode projectNode
            //    = AddChildIfItDoesNotExist(currentParentNode, project);
            //    SimpleMutableClassificationNodeMinimalWithIdNode retRoot =
            //    AddTreeRecursive(projectNode, simpleMutableClassificationNodeMinimalWithIdNode);
            //    return retRoot;
            //}
            SimpleMutableClassificationNodeMinimalWithIdNode lastNodeAdded =
                AddTreeRecursive(currentParentNode, simpleMutableClassificationNodeMinimalWithIdNode);
            return lastNodeAdded;
        }

        private SimpleMutableClassificationNodeMinimalWithIdNode AddTreeRecursive(
            SimpleMutableClassificationNodeMinimalWithIdNode parent,
            SimpleMutableClassificationNodeMinimalWithIdNode treeToAdd)
        {
            SimpleMutableClassificationNodeMinimalWithIdNode nextParent = null;
            var nodeToInsertFound = parent.Children.Any(n => n.Name == treeToAdd.Item.Name);
            if (nodeToInsertFound)
            {
                //already found
                nextParent = parent.Children.Single(n => n.Name == treeToAdd.Item.Name);
            }
            else
            {
                //not found so add it
                ClassificationNodeMinimalWithIdItem item = new ClassificationNodeMinimalWithIdItem(
                    parent.Root.Max(n => n.Item.Id) + 1,
                    treeToAdd.Item.Name,
                    treeToAdd.Item.StructureType,
                    treeToAdd.Item.HasChildren,
                    $"{parent.Item.Path}\\{treeToAdd.Item.Name}",
                    treeToAdd.Item.Attributes
                    );
                nextParent = parent.AddChild(item);
            }
            SimpleMutableClassificationNodeMinimalWithIdNode retRoot = nextParent;
            //now add children
            foreach (var childTreeToAdd in treeToAdd.Children)
            {
                AddTreeRecursive(nextParent, childTreeToAdd);
            }
            return retRoot;
        }        

        private SimpleMutableClassificationNodeMinimalWithIdNode AddChildIfItDoesNotExist(
            SimpleMutableClassificationNodeMinimalWithIdNode parent,
            string portfolio)
        {
            SimpleMutableClassificationNodeMinimalWithIdNode portfolioNode = null;
            var portfolioFound = parent.Children.Any(n => n.Name == portfolio);
            if (portfolioFound)
            {
                portfolioNode = parent.Children.Single(n => n.Name == portfolio);
            }
            else
            {
                ClassificationNodeMinimalWithIdItem item = new ClassificationNodeMinimalWithIdItem(
                    parent.Root.Max(n => n.Item.Id) + 1,
                    portfolio,
                    parent.Item.StructureType,
                    false,
                    $"{parent.Item.Path}\\{portfolio}",
                    null
                    );
                portfolioNode = parent.AddChild(item);
            }
            return portfolioNode;
        }

        public SimpleMutableClassificationNodeMinimalWithIdNode GetSubTree(string subPath, string structureType)
        {
            string normalizedPath = GetNormalizedPath(subPath, structureType);
            var subTree = this.Single(node => node.Item.Path == normalizedPath);
            //subTree.Detach(); //do we really need this detached??? DESTRUCTIVE (it's actually removing the subtree)! so no detach
            return subTree;
        }

        public string GetNormalizedPath(string subPath, string structureType)
        {
            var nodes = subPath.Split(new string[] { Constants.DefaultPathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            var normalizedPath = $"{Constants.DefaultPathSeparator}{nodes.First()}{Constants.DefaultPathSeparator}{structureType}";
            if (nodes.Length > 1)
            {
                normalizedPath += $"{Constants.DefaultPathSeparator}{String.Join(Constants.DefaultPathSeparator, nodes.Skip(1))}";
            }
            return normalizedPath;
        }

        public bool HasPath(string pathToFind, string structureType)
        {
            if (!IsNormalizedPath(pathToFind, structureType))
            {
                pathToFind = GetNormalizedPath(pathToFind, structureType);
            }
            return this.Any(node => node.Item.Path == pathToFind && node.Item.StructureType == structureType.ToLower());
        }

        public bool IsNormalizedPath(string pathToFind, string structureType)
        {
            var pathParts = pathToFind.Split(new string[] { Constants.DefaultPathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            return pathParts.Length > 1 && pathParts[1] == structureType;

        }

        public SimpleMutableClassificationNodeMinimalWithIdNode GetNodeWithPath(string pathToFind, string structureType)
        {
            if (!IsNormalizedPath(pathToFind, structureType))
            {
                pathToFind = GetNormalizedPath(pathToFind, structureType);
            }
            return this.SingleOrDefault(node => node.Item.Path == pathToFind && node.Item.StructureType == structureType.ToLower());
        }


    }
}
