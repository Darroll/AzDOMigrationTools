using System.Collections.Generic;
using System.IO;
using System.Linq;
using ADO.Engine.BusinessEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ADO.Configuration.Tests
{
    [TestClass]
    public class TestBusinessEntities
    {
        [TestMethod]
        public void Test_Serialize_Business_Hierarchy()
        {
            BusinessHierarchy businessHierarchy = new BusinessHierarchy()
            {
                Name = "OneProjectV2_02",
                Children = new List<BusinessHierarchy>()
                {
                    new BusinessHierarchy()
                    {
                        Name = "Customer",
                        BusinessHierarchyTeam = new BusinessHierarchyTeam()
                        {
                            Name="Customer Portfolio Team",
                        },
                        Children = new List<BusinessHierarchy>()
                        {
                            new BusinessHierarchy()
                            {
                                Name="FinTech",
                                BusinessHierarchyTeam = new BusinessHierarchyTeam()
                                {
                                    Name="FinTech Program Team"
                                },
                                Children = new List<BusinessHierarchy>()
                                {
                                    new BusinessHierarchy()
                                    {
                                        OrganizationOrCollectionName="devopsabcs",
                                        Name="Finastra-Hackfest-PoC"
                                    },
                                }
                            }
                        }
                    },
                    new BusinessHierarchy()
                    {
                        Name = "Demo",
                        BusinessHierarchyTeam = new BusinessHierarchyTeam()
                        {
                            Name="Demo Portfolio Team",
                        },
                        Children = new List<BusinessHierarchy>()
                        {
                            new BusinessHierarchy()
                            {
                                Name="Migrations",
                                BusinessHierarchyTeam = new BusinessHierarchyTeam()
                                {
                                    Name="Migrations Program Team"
                                },
                                Children = new List<BusinessHierarchy>()
                                {
                                    new BusinessHierarchy()
                                    {
                                        OrganizationOrCollectionName="devopsabcs",
                                        Name="PartsUnlimited"
                                    },
                                    new BusinessHierarchy()
                                    {
                                        OrganizationOrCollectionName="devopsabcs",
                                        Name="SmartHotel360"
                                    }
                                }
                            }
                        }
                    },
                    new BusinessHierarchy()
                    {
                        Name = "Research",
                        BusinessHierarchyTeam = new BusinessHierarchyTeam()
                        {
                            Name="Research Portfolio Team",
                        },
                        Children = new List<BusinessHierarchy>()
                        {
                            new BusinessHierarchy()
                            {
                                Name="Migrations",
                                BusinessHierarchyTeam = new BusinessHierarchyTeam()
                                {
                                    Name="Migrations Program Team"
                                },
                                Children = new List<BusinessHierarchy>()
                                {
                                    new BusinessHierarchy()
                                    {
                                        OrganizationOrCollectionName="devopsabcs",
                                        Name="DevOpsDemoGenerator"
                                    },
                                    new BusinessHierarchy()
                                    {
                                        OrganizationOrCollectionName="devopsabcs",
                                        Name="MigrationTool"
                                    }
                                }
                            },
                        }
                    },
                    new BusinessHierarchy()
                    {
                        Name = "Training",
                        BusinessHierarchyTeam = new BusinessHierarchyTeam()
                        {
                            Name="Training Portfolio Team",
                        },
                        Children = new List<BusinessHierarchy>()
                        {
                            new BusinessHierarchy()
                            {
                                Name="CertAZ203",
                                BusinessHierarchyTeam = new BusinessHierarchyTeam()
                                {
                                    Name="CertAZ203 Program Team"
                                },
                                Children = new List<BusinessHierarchy>()
                                {
                                    new BusinessHierarchy()
                                    {
                                        OrganizationOrCollectionName="devopsabcs",
                                        Name="ApplicationInsights"
                                    },
                                    new BusinessHierarchy()
                                    {
                                        OrganizationOrCollectionName="devopsabcs",
                                        Name="AutoScaling"
                                    }
                                }
                            }
                        }
                    },
                    new BusinessHierarchy()
                    {
                        Name = "Workshops",
                        BusinessHierarchyTeam = new BusinessHierarchyTeam()
                        {
                            Name="Workshops Portfolio Team",
                        },
                        Children = new List<BusinessHierarchy>()
                        {
                            new BusinessHierarchy()
                            {
                                Name="DevOps",
                                BusinessHierarchyTeam = new BusinessHierarchyTeam()
                                {
                                    Name="DevOps Program"
                                },
                                Children = new List<BusinessHierarchy>()
                                {
                                    new BusinessHierarchy()
                                    {
                                        OrganizationOrCollectionName="devopsabcs",
                                        Name="hol"
                                    },
                                    new BusinessHierarchy()
                                    {
                                        OrganizationOrCollectionName="devopsabcs",
                                        Name="hol-tbs-3"
                                    }
                                }
                            }
                        }
                    }
                }
            };
            string json = JsonConvert.SerializeObject(businessHierarchy, Formatting.Indented);
            File.WriteAllText(@"..\..\Output\businessHierarchy.json", json);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Hydrate_Business_Hierarchy()
        {
            string json = File.ReadAllText(@"..\..\Input\businessHierarchy.json");
            BusinessHierarchy root
                = JsonConvert.DeserializeObject<BusinessHierarchy>(json);

            Assert.IsNotNull(root);
        }

        [TestMethod]
        public void Test_Get_Area_Paths()
        {
            string json = File.ReadAllText(@"..\..\Input\businessHierarchyFromAreas.json");

            var businessHierarchy = JsonConvert.DeserializeObject<BusinessHierarchy>(json);

            var areasFromBuildHierarchy = businessHierarchy.GetAreas();


            File.WriteAllLines(@"..\..\Output\areasFromBuildHierarchy.txt", areasFromBuildHierarchy);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_Get_Area_Paths_From_BusinessNode_Mutable_Tree()
        {
            SimpleMutableBusinessNodeNode root = SimpleMutableBusinessNodeNode.LoadFromJson(@"..\..\Input\businessNodeFromAreas.json");

            var areasFromBuildNode = root.SelectDescendants().Select(bNode => bNode.GetAreaOrIterationPath(Constants.AreaStructureType, true, true));

            File.WriteAllLines(@"..\..\Output\areasFromBuildNode.txt", areasFromBuildNode);
            Assert.IsTrue(true);
        }
    }
}
