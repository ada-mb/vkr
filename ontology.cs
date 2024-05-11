using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PracticeCSharp
{
    class Class8
    {
        public class ontologyClass
        {
            public XName name; //стринг неявно преобразовывается в xelement
            public ontologyClass parent;
            public ontologyClass(String name1, ontologyClass parent1)
            {
                name = name1;
                parent = parent1;
            }
            public ontologyClass(String name1)
            {
                name = name1;
                parent = null;
            }
            public bool matching(ontologyClass cl)
            {
                if (this.name == cl.name)
                    return true;
                else if (cl.parent != null)
                    return this.matching(cl.parent);
                else
                    return false;
            }
        }
        public class enumerationType
        {
            public XName name;
            public XElement xml;
            public enumerationType(XName name1, XElement xml1)
            {
                name = name1;
                xml = xml1;
            }
        }
        public class datatypePredicate
        {
            public XName name;
            public ontologyClass subj;
            public enumerationType enumeretion; //если null, то значит просто текст
            public datatypePredicate(XName name1, ontologyClass subj1, enumerationType en1)
            {
                name = name1;
                subj = subj1;
                enumeretion = en1;
            }
        }
        public class objectPredicate
        {
            public XName name;
            public ontologyClass subj;
            public ontologyClass obj; 
            public objectPredicate(XName name1, ontologyClass subj1, ontologyClass obj1)
            {
                name = name1;
                subj = subj1;
                obj = obj1;
            }
        }


        public static void Main(String[] args)
        {
            XElement ontology = XElement.Load(@"C:\Users\admat\source\repos\с#\PracticeCSharp\Ontology_iis-v14.xml");
            List<ontologyClass> classes = new List<ontologyClass>();
            List<enumerationType> enumerationTypes = new List<enumerationType>();
            List<datatypePredicate> dataPredicates = new List<datatypePredicate>();
            List<objectPredicate> objectPredicates = new List<objectPredicate>();
            int k = 0;
            foreach (XElement xml in ontology.Elements())
            {
                //Console.WriteLine(xml + "\n");
                //Console.WriteLine("Name = "+ xml.Name + "\n");

                if (xml.Name == "Class")
                {
                    k++;
                    //Console.WriteLine(xml + "\n");

                    String id = xml.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                    String name = id.Substring(id.LastIndexOf('/') + 1);
                    XElement parrent = xml.Elements().FirstOrDefault(x => x.Name == "SubClassOf");
                    ontologyClass parrentClass = null;
                    if (parrent != null)
                    {
                        String parrentId = parrent.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value;
                        String parrentName = parrentId.Substring(parrentId.LastIndexOf('/') + 1);
                        parrentClass = classes.Find(x => x.name == parrentName);
                    }
                    //Console.WriteLine("NameOntClass = " + name + "\n");
                    ontologyClass ontClass = new ontologyClass(name, parrentClass);
                    classes.Add(ontClass);
                } else if (xml.Name == "EnumerationType")
                {
                    //Console.WriteLine(xml + "\n");
                    String id = xml.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                    String name = id.Substring(id.LastIndexOf('/') + 1);
                    enumerationTypes.Add(new enumerationType(name, xml));
                } else if (xml.Name == "DatatypeProperty")
                {
                    //Console.WriteLine(xml + "\n");
                    String id = xml.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                    String name = id.Substring(id.LastIndexOf('/') + 1);
                    XElement range = xml.Elements().FirstOrDefault(x => x.Name == "range");
                    String idRange = range.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value;
                    String nameRange = idRange.Substring(idRange.LastIndexOf('/') + 1);
                    //Console.WriteLine(nameRange + "\n");
                    enumerationType enumeration = enumerationTypes.FirstOrDefault(x => x.name == nameRange);

                    XElement domain = xml.Elements().FirstOrDefault(x => x.Name == "domain");
                    String idDomain = domain.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value;
                    String nameDomain = idDomain.Substring(idDomain.LastIndexOf('/') + 1);
                    //Console.WriteLine(nameDomain + "\n");
                    ontologyClass cl = classes.FirstOrDefault(x => x.name == nameDomain);
                    if (cl == null)
                    {
                        Console.WriteLine("not founnd " + nameDomain + "class");
                    }
                    dataPredicates.Add(new datatypePredicate(name, cl, enumeration));
                } else if (xml.Name == "ObjectProperty")
                {
                    String id = xml.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                    String name = id.Substring(id.LastIndexOf('/') + 1);
                    XElement range = xml.Elements().FirstOrDefault(x => x.Name == "range");
                    String idRange = range.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value;
                    String nameRange = idRange.Substring(idRange.LastIndexOf('/') + 1);
                    //Console.WriteLine(nameRange + "\n");
                    ontologyClass obj = classes.FirstOrDefault(x => x.name == nameRange);
                    if (obj == null)
                    {
                     //   Console.WriteLine("not founnd " + nameRange + " class");
                    }

                    XElement domain = xml.Elements().FirstOrDefault(x => x.Name == "domain");
                    String idDomain = domain.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value;
                    String nameDomain = idDomain.Substring(idDomain.LastIndexOf('/') + 1);
                    //Console.WriteLine(nameDomain + "\n");
                    ontologyClass subj = classes.FirstOrDefault(x => x.name == nameDomain);
                    if (subj == null)
                    {
                        //Console.WriteLine("not founnd " + nameDomain + " class, xml:");
                        subj = new ontologyClass("archive-member", classes.FirstOrDefault(x => x.name == "entity"));
                    }
                    objectPredicates.Add(new objectPredicate(name, subj, obj));
                }
            }
            XElement dataBase = XElement.Load(@"C:\Users\admat\source\repos\с#\PracticeCSharp\SypCassete_current.fog");
            List<XName> notFoundedDatatypePredicates = new List<XName>();
            List<XName> notFoundedObjectPredicates = new List<XName>();
            List<datatypePredicate> FoundedDatatypePredicates = new List<datatypePredicate>();
            List<objectPredicate> FoundedObjectPredicates = new List<objectPredicate>();
            List<ontologyClass> FoundedClasses = new List<ontologyClass>();
            Func<String, XElement> idSearch = str => dataBase.Elements()
               .FirstOrDefault(elm => elm.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value == str);
            foreach (XElement xml in dataBase.Elements())
            {
                XName name = xml.Name;
                ontologyClass className = classes.FirstOrDefault(x => x.name == name);
                FoundedClasses.Add(className);
                foreach (XElement field in xml.Elements())
                {
                    XName pName = field.Name;
                    var resource = field.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource");
                    if (resource == null)
                    {
                        datatypePredicate p = dataPredicates.FirstOrDefault(pr => pr.name == pName);
                        if (p != null)
                        {
                            FoundedDatatypePredicates.Add(p);
                            if (!p.subj.matching(className))
                            {
                                Console.WriteLine("Класс элемента не соответсвует классу предиката");
                                Console.WriteLine(xml);
                            }
                            if (p.enumeretion != null)
                            {
                                Console.WriteLine(xml);
                            }
                        }
                        else
                        {
                            //Console.WriteLine("no found datatype predicate " + pName);
                            notFoundedDatatypePredicates.Add(pName);
                        }
                    }
                    else if (resource != null)
                    {
                        objectPredicate p = objectPredicates.FirstOrDefault(pr => pr.name == pName);
                        if (p != null)
                        {
                            FoundedObjectPredicates.Add(p);
                            if (!p.subj.matching(className))
                            {
                                Console.WriteLine("Класс элемента не соответсвует классу предиката");
                                Console.WriteLine(xml);
                            }
                            String idResource = resource.Value;
                            XElement obj = idSearch(idResource);
                            if (obj != null)
                            {
                                XName objName = obj.Name;
                                ontologyClass objClass = classes.FirstOrDefault(c => c.name == objName);
                                if (!p.obj.matching(objClass))
                                {
                                    Console.WriteLine("Класс объекта,  на который ссылается элемент, не соответсвует допустимому объектному классу для этого предиката");
                                    Console.WriteLine(xml);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("no found object predicate " + pName);
                            notFoundedObjectPredicates.Add(pName);
                        }
                    }                    
                }
            }
            Console.WriteLine(notFoundedDatatypePredicates.Distinct().Count() + " не обнаруженных datatype предикатов:");
            foreach (XName name in notFoundedDatatypePredicates.Distinct())
            {
                Console.WriteLine(name);
            }
            Console.WriteLine(FoundedDatatypePredicates.Distinct().Count() + " обнаруженных datatype предикатов:");
            foreach (datatypePredicate pr in FoundedDatatypePredicates.Distinct())
            {
                Console.WriteLine(pr.name);
            }

            Console.WriteLine(notFoundedObjectPredicates.Distinct().Count() + " не обнаруженных object предикатов:");
            foreach (XName name in notFoundedObjectPredicates.Distinct())
            {
                Console.WriteLine(name);
            }
            
            Console.WriteLine(FoundedObjectPredicates.Distinct().Count() + " обнаруженных object предикатов:");
            foreach (objectPredicate pr in FoundedObjectPredicates.Distinct())
            {
                Console.WriteLine(pr.name);
            }
            /*
            foreach (objectPredicate pred in objectPredicates)
            {
                Console.WriteLine(pred.name + " " + pred.subj.name + " " + pred.obj.name);
            }
            */
            /*
            foreach (var cl in ontologyClasses)
            {
                Console.Write(cl.name + " ");
                if (cl.parent != null)
                    Console.Write(cl.parent.name);
                Console.WriteLine();
            }
            Console.WriteLine(k + " " + ontologyClasses.Count);
            */
            /*
            List<XName> ontologyElementsVariations = new List<XName>();
            foreach (XElement xml in ontology.Elements())
            {
                ontologyElementsVariations.Add(xml.Name);
            }
            foreach (XName name in ontologyElementsVariations.Distinct())
            {
                Console.WriteLine(name);
            }
            */
        }
    }
}
