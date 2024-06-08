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
            public XName name; //стринг неявно преобразовывается в xname
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
            Dictionary<String, ontologyClass> classes = new Dictionary<String, ontologyClass>();
            Dictionary<String, enumerationType> enumerationTypes = new Dictionary<String, enumerationType>();
            Dictionary<String, datatypePredicate> dataPredicates = new Dictionary<String, datatypePredicate>();
            Dictionary<String, objectPredicate> objectPredicates = new Dictionary<String, objectPredicate>();
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
                        classes.TryGetValue(parrentName, out parrentClass);
                    }
                    //Console.WriteLine("NameOntClass = " + name + "\n");
                    ontologyClass ontClass = new ontologyClass(name, parrentClass);
                    classes.Add(name, ontClass);
                } else if (xml.Name == "EnumerationType")
                {
                    //Console.WriteLine(xml + "\n");
                    String id = xml.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                    String name = id.Substring(id.LastIndexOf('/') + 1);
                    enumerationTypes.Add(name, new enumerationType(name, xml));
                } else if (xml.Name == "DatatypeProperty")
                {
                    //Console.WriteLine(xml + "\n");
                    String id = xml.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                    String name = id.Substring(id.LastIndexOf('/') + 1);
                    XElement range = xml.Elements().FirstOrDefault(x => x.Name == "range");
                    String idRange = range.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value;
                    String nameRange = idRange.Substring(idRange.LastIndexOf('/') + 1);
                    //Console.WriteLine(nameRange + "\n");
                    enumerationType enumeration = null;
                    enumerationTypes.TryGetValue(nameRange, out enumeration);

                    XElement domain = xml.Elements().FirstOrDefault(x => x.Name == "domain");
                    String idDomain = domain.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value;
                    String nameDomain = idDomain.Substring(idDomain.LastIndexOf('/') + 1);
                    //Console.WriteLine(nameDomain + "\n");
                    ontologyClass cl = null;
                    classes.TryGetValue(nameDomain, out cl);
                    if (cl == null)
                    {
                        Console.WriteLine("not founnd " + nameDomain + "class");
                    }
                    dataPredicates.Add(name, new datatypePredicate(name, cl, enumeration));
                } else if (xml.Name == "ObjectProperty")
                {
                    String id = xml.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                    String name = id.Substring(id.LastIndexOf('/') + 1);
                    XElement range = xml.Elements().FirstOrDefault(x => x.Name == "range");
                    String idRange = range.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value;
                    String nameRange = idRange.Substring(idRange.LastIndexOf('/') + 1);
                    //Console.WriteLine(nameRange + "\n");
                    ontologyClass obj = null; 
                    classes.TryGetValue(nameRange, out obj);

                    if (obj == null)
                    {
                     //   Console.WriteLine("not founnd " + nameRange + " class");
                    }

                    XElement domain = xml.Elements().FirstOrDefault(x => x.Name == "domain");
                    String idDomain = domain.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource").Value;
                    String nameDomain = idDomain.Substring(idDomain.LastIndexOf('/') + 1);
                    //Console.WriteLine(nameDomain + "\n");
                    ontologyClass subj = null;
                    classes.TryGetValue(nameDomain, out subj);
                    if (subj == null)
                    {
                        //Console.WriteLine("not founnd " + nameDomain + " class, xml:");
                        ontologyClass entityClass = null; //parrent cl для archive-member
                        classes.TryGetValue("entity", out entityClass);
                        subj = new ontologyClass("archive-member", entityClass);
                    }
                    objectPredicates.Add(name, new objectPredicate(name, subj, obj));
                }
            }
            //XElement dataBase = XElement.Load(@"C:\Users\admat\source\repos\с#\PracticeCSharp\SypCassete_current.fog");
            XElement dataBase = XElement.Load(@"C:\Users\admat\source\repos\с#\PracticeCSharp\sypcollection.xml");
            List<XName> notFoundedDatatypePredicates = new List<XName>();
            List<XName> notFoundedObjectPredicates = new List<XName>();
            List<XName> notFoundedClasses = new List<XName>();
            List<datatypePredicate> FoundedDatatypePredicates = new List<datatypePredicate>();
            List<objectPredicate> FoundedObjectPredicates = new List<objectPredicate>();
            List<ontologyClass> FoundedClasses = new List<ontologyClass>();
            Func<String, XElement> idSearch = str => dataBase.Elements()
               .FirstOrDefault(elm => elm.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value == str);
            foreach (XElement xml in dataBase.Elements().Take(5))
            {                
                XName name = xml.Name;
                ontologyClass className = null;
                classes.TryGetValue(name.ToString(), out className);
                if (className != null)
                    FoundedClasses.Add(className);
                else
                {
                    notFoundedClasses.Add(name);
                    continue;
                }
                foreach (XElement field in xml.Elements())
                {
                    XName pName = field.Name;
                    var resource = field.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource");
                    if (resource == null)
                    {
                        datatypePredicate p = null; 
                        dataPredicates.TryGetValue(pName.ToString(), out p);
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
                        objectPredicate p = null;
                        objectPredicates.TryGetValue(pName.ToString(), out p);
                        if (p != null)
                        {
                            FoundedObjectPredicates.Add(p);
                            if (!p.subj.matching(className))
                            {
                                Console.WriteLine("Класс элемента не соответствует классу предиката");
                                Console.WriteLine(xml);
                            }
                            String idResource = resource.Value;
                            XElement obj = idSearch(idResource);
                            if (obj != null)
                            {
                                XName objName = obj.Name;
                                ontologyClass objClass = null; 
                                classes.TryGetValue(objName.ToString(), out objClass);
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
            
            Console.WriteLine(notFoundedClasses.Distinct().Count() + " не обнаруженных classes:");
            foreach (XName name in notFoundedClasses.Distinct())
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();

            Console.WriteLine(FoundedClasses.Distinct().Count() + " обнаруженных classes:");
            foreach (ontologyClass cl in FoundedClasses.Distinct())
            {
                Console.WriteLine(cl.name);
            }
            Console.WriteLine();

            Console.WriteLine(classes.Count() + " содержит онтология\n");

            Console.WriteLine(notFoundedDatatypePredicates.Distinct().Count() + " не обнаруженных datatype предикатов:");
            foreach (XName name in notFoundedDatatypePredicates.Distinct())
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();

            Console.WriteLine(FoundedDatatypePredicates.Distinct().Count() + " обнаруженных datatype предикатов:");
            foreach (datatypePredicate pr in FoundedDatatypePredicates.Distinct())
            {
                Console.WriteLine(pr.name);
            }
            Console.WriteLine();
            Console.WriteLine(dataPredicates.Count() + " содержит онтология\n");

            Console.WriteLine(notFoundedObjectPredicates.Distinct().Count() + " не обнаруженных object предикатов:");
            foreach (XName name in notFoundedObjectPredicates.Distinct())
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();

            Console.WriteLine(FoundedObjectPredicates.Distinct().Count() + " обнаруженных object предикатов:");
            foreach (objectPredicate pr in FoundedObjectPredicates.Distinct())
            {
                Console.WriteLine(pr.name);
            }
            Console.WriteLine();
            Console.WriteLine(objectPredicates.Count() + " содержит онтология\n");

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
