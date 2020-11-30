using ADO.Engine.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeCollections;

namespace ADO.Engine.BusinessEntities
{
    public class ClassificationNodeMinimal
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "structureType")]
        public string StructureType { get; set; }

        [JsonProperty(PropertyName = "hasChildren")]
        public bool HasChildren { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public Attributes Attributes { get; set; }

        [JsonProperty(PropertyName = "children")]
        public List<ClassificationNodeMinimal> Children { get; set; }

        public SimpleMutableClassificationNodeMinimalWithIdNode ToSimpleMutableClassificationNodeMinimalWithIdNode(
            out Dictionary<int, SimpleMutableClassificationNodeMinimalWithIdNode> mapFromClassNodeIdsToTreeNodes)
        {
            //first we need to inject id!!!
            ClassificationNodeMinimalWithId classificationNodeMinimalWithId = this.ToClassificationNodeMinimalWithId();
            //now we convert to data node
            string json = JsonConvert.SerializeObject(classificationNodeMinimalWithId, Formatting.Indented);
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
            mapFromClassNodeIdsToTreeNodes =
                root.Select(tn => tn).ToDictionary(k => k.Item.Id, v => v);
            return root;
        }

        public ClassificationNodeMinimalWithId ToClassificationNodeMinimalWithId()
        {
            int counter = 1;
            ClassificationNodeMinimalWithId newClassificationNodeMinimalWithId = new ClassificationNodeMinimalWithId();
            PreOrderTraversal(newClassificationNodeMinimalWithId, this, ref counter);
            return newClassificationNodeMinimalWithId;
        }

        public void PreOrderTraversal(ClassificationNodeMinimalWithId newClassificationNodeMinimalWithId, ClassificationNodeMinimal classificationNodeMinimal, ref int counter)
        {
            //if (node == NULL)
            //    return;
            newClassificationNodeMinimalWithId.Id = counter++;
            newClassificationNodeMinimalWithId.Name = classificationNodeMinimal.Name;
            newClassificationNodeMinimalWithId.Path = classificationNodeMinimal.Path;
            newClassificationNodeMinimalWithId.StructureType = classificationNodeMinimal.StructureType;
            newClassificationNodeMinimalWithId.HasChildren = classificationNodeMinimal.HasChildren;
            newClassificationNodeMinimalWithId.Attributes = classificationNodeMinimal.Attributes;
            newClassificationNodeMinimalWithId.Children = new List<ClassificationNodeMinimalWithId>(); //ensure empty list of children always

            if (classificationNodeMinimal.HasChildren || (classificationNodeMinimal.Children != null && classificationNodeMinimal.Children.Count > 0))
            {
                foreach (var child in classificationNodeMinimal.Children)
                {
                    ClassificationNodeMinimalWithId newChild = new ClassificationNodeMinimalWithId();
                    newClassificationNodeMinimalWithId.Children.Add(newChild);
                    PreOrderTraversal(newChild, child, ref counter);
                }
            }
        }

        public static ClassificationNodeMinimal LoadFromJson(string finalAreaHierarchyPath)
        {
            return finalAreaHierarchyPath.LoadFromJson<ClassificationNodeMinimal>();
        }

        public ClassificationNodeMinimal AddTree(
            //string portfolio, string programOrProduct,
            //string project,
            List<string> prefixedNodeNames,
            SimpleMutableClassificationNodeMinimalWithIdNode simpleMutableClassificationNodeMinimalWithIdNode)
        {
            //ClassificationNodeMinimal initialRoot = this;
            ClassificationNodeMinimal currentParentNode = this;
            if (prefixedNodeNames != null && prefixedNodeNames.Count > 0)
            {
                foreach (var prefixedNodeName in prefixedNodeNames)
                {
                    currentParentNode
                        = AddChildIfItDoesNotExist(currentParentNode, prefixedNodeName);
                }
                //ClassificationNodeMinimal portfolioNode
                //= AddChildIfItDoesNotExist(this, portfolio);
                //ClassificationNodeMinimal programOrProductNode
                //    = AddChildIfItDoesNotExist(portfolioNode, programOrProduct);                
            }
            else
            {
                //throw new NotImplementedException();
            }
            

            //if (string.IsNullOrEmpty(project))
            //{
            //    ClassificationNodeMinimal retRoot =
            //AddTreeRecursive(programOrProductNode, simpleMutableClassificationNodeMinimalWithIdNode);
            //    return retRoot;
            //}
            //else
            //{
            //    ClassificationNodeMinimal projectNode
            //    = AddChildIfItDoesNotExist(programOrProductNode, project);
            //    ClassificationNodeMinimal retRoot =
            //    AddTreeRecursive(projectNode, simpleMutableClassificationNodeMinimalWithIdNode);
            //    return retRoot;
            //}

            ClassificationNodeMinimal lastNodeAdded =
                AddTreeRecursive(currentParentNode, simpleMutableClassificationNodeMinimalWithIdNode);
            return lastNodeAdded;
        }

        private ClassificationNodeMinimal AddTreeRecursive(ClassificationNodeMinimal parent,
            SimpleMutableClassificationNodeMinimalWithIdNode treeToAdd)
        {
            ClassificationNodeMinimal nextParent = null;
            var nodeToInsertFound = parent.Children.Any(n => n.Name == treeToAdd.Item.Name);
            if (nodeToInsertFound)
            {
                //already found
                nextParent = parent.Children.Single(n => n.Name == treeToAdd.Item.Name);
            }
            else
            {
                //not found so add it
                ClassificationNodeMinimal item = new ClassificationNodeMinimal()
                {
                    Name = treeToAdd.Item.Name,
                    StructureType = treeToAdd.Item.StructureType,
                    HasChildren = false,// treeToAdd.Item.HasChildren,
                    Path = $"{parent.Path}\\{treeToAdd.Item.Name}",
                    Attributes = treeToAdd.Item.Attributes,
                    Children = new List<ClassificationNodeMinimal>()
                };
                parent.Children.Add(item);
                parent.HasChildren = true;
                nextParent = item;
            }
            ClassificationNodeMinimal retRoot = nextParent;
            //now add children
            foreach (var childTreeToAdd in treeToAdd.Children)
            {
                AddTreeRecursive(nextParent, childTreeToAdd);
            }
            return retRoot;
        }

        private ClassificationNodeMinimal AddChildIfItDoesNotExist(ClassificationNodeMinimal parent,
            string portfolio)
        {
            ClassificationNodeMinimal portfolioNode = null;
            var portfolioFound = parent.Children.Any(n => n.Name == portfolio);
            if (portfolioFound)
            {
                portfolioNode = parent.Children.Single(n => n.Name == portfolio);
            }
            else
            {
                ClassificationNodeMinimal item = new ClassificationNodeMinimal()
                {
                    Name = portfolio,
                    Attributes = null,
                    HasChildren = false,
                    Path = $"{parent.Path}\\{portfolio}",
                    StructureType = parent.StructureType,
                    Children = new List<ClassificationNodeMinimal>()
                };
                parent.Children.Add(item);
                parent.HasChildren = true;
                portfolioNode = item;
            }
            return portfolioNode;
        }

        public void RenameRoot(string newClassificationRootName)
        {
            RenameRootRecursive(newClassificationRootName, true);
        }
        private void RenameRootRecursive(string newClassificationRootName, bool isRoot)
        {
            if (isRoot)
            {
                this.Name = newClassificationRootName;
            }
            this.Path = ClassificationNodeMinimal.RenamePath(this.Path, newClassificationRootName);
            if (this.Children != null)
            {
                foreach (var child in this.Children)
                {
                    child.RenameRootRecursive(newClassificationRootName, false);
                }
            }
        }

        public static string RenamePath(string path, string newClassificationRootName)
        {
            var pathParts = path.Split(new string[] { Constants.DefaultPathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            pathParts[0] = newClassificationRootName;
            string retPath = $"{Constants.DefaultPathSeparator}{String.Join(Constants.DefaultPathSeparator, pathParts)}";
            return retPath;
        }
    }
}
