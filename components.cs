using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PracticeCSharp
{
    class Class7
    {
        public class Edge
        {
            public Node obj;
            public XName pred;
            public Edge(Node obj1, XName prdc1)
            {
                obj = obj1;
                pred = prdc1;
            }
        }
        public class DanglingEdge
        {
            public String obj;
            public XName pred;
            public DanglingEdge(String obj1, XName prdc1)
            {
                obj = obj1;
                pred = prdc1;
            }
        }
        public class DanglingLink
        {
            public Node sub;//в кач субъекта храним узел (а не XElement) чтобы сразу можно было понять в какую компоненту связ вх ссылка/
                            //какие еще висячие ссылки есть у этого объекта и другие шткуи, которые можно узнать с помощью
                            // построенного графа. так как и XElement и Node - reference типы,затраты по памяти соразмерны
            public String obj;
            public XName pred;
            public DanglingLink(Node sub1, String obj1, XName prdc1)
            {
                sub = sub1;
                obj = obj1;
                pred = prdc1;
            }
        }
        public class Node
        {
            public String id;
            public XElement xml;
            public IEnumerable<Edge> outbox;
            public IEnumerable<Edge> inbox;
            public bool flag;
            public IEnumerable<DanglingEdge> outboxNOInBase;
            public Node(String id1, XElement xml1)
            {
                id = id1;
                xml = xml1;
                outbox = null;
                inbox = null;
                flag = false;
                outboxNOInBase = null;
            }
        }
        public static void DFS(Dictionary<String, Node> nodes, Node nod, Dictionary<String, Node> component)
        {
            nod.flag = true;
            component.Add(nod.id, nod);
            if (nod.outbox != null)
            {
                foreach (Edge parrent in nod.outbox)
                {
                    if (parrent.obj.flag != true)
                    {
                        DFS(nodes, parrent.obj, component);
                    }
                }
            }
            if (nod.inbox != null)
            {
                foreach (Edge child in nod.inbox)
                {
                    if (child.obj.flag != true)
                    {
                        DFS(nodes, child.obj, component);
                    }
                }
            }
        }
        public static void Main(String[] args)
        {
            //XElement dataBase = XElement.Load(@"C:\Users\admat\source\repos\с#\PracticeCSharp\SypCassete_current.fog");
            //XElement dataBase = XElement.Load(@"C:\Users\admat\source\repos\с#\PracticeCSharp\test2.fog");
            XElement dataBase = XElement.Load(@"C:\Users\admat\source\repos\с#\PracticeCSharp\sypcollection.xml");
            int numberOfElementsInDatabase = dataBase.Elements().Count();
            Dictionary<String, XElement> elements = new Dictionary<String, XElement>(numberOfElementsInDatabase);

            Dictionary<String, Node> nodes = new Dictionary<String, Node>(numberOfElementsInDatabase);
            //преимущество словаря - поиск элемента по ключу(то  есть по id) за o(1)

            //1) создаем для каждого xml свой nodе
            foreach (XElement xml in dataBase.Elements())
            {
                String id = xml.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                Node nod = new Node(id, xml); //мб надо вообще поле id убрать из Node?нет,для единообразия с edge оставим
                nodes.Add(id, nod);
            }


            //2) идем по nodes и соединяем их с другими nodes
            //побочным действием алгоритма явл выявление висящих ссылок во всей базе, которые мы будем хранить в danglingLinks
            List<DanglingLink> danglingLinks = new List<DanglingLink>();
            List<String> notFoundedIds = new List<string>(); 
            foreach (KeyValuePair<String, Node> kvp in nodes)
            {
                Node nod = kvp.Value;
                String id = kvp.Key;
                foreach (var field in nod.xml.Elements())
                {
                    var resource = field.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource");
                    if (resource != null)
                    {
                        XName predicate = field.Name; 
                        String parrentId = resource.Value;
                        {
                            Node parrentNod = null;
                            bool founded = nodes.TryGetValue(parrentId, out parrentNod);//в среднем o(1) поиск по всей базе узла с таким id
                            if (founded)
                            {
                                if (parrentNod.inbox == null)
                                {
                                    parrentNod.inbox = Enumerable.Empty<Edge>();
                                }
                                if (nod.outbox == null)
                                {
                                    nod.outbox = Enumerable.Empty<Edge>();
                                }
                                parrentNod.inbox = parrentNod.inbox.Append(new Edge(nod, predicate));
                                nod.outbox = nod.outbox.Append(new Edge(parrentNod, predicate));
                            }
                            else
                            {
                                if (nod.outboxNOInBase == null)
                                {
                                    nod.outboxNOInBase = Enumerable.Empty<DanglingEdge>();
                                }
                                nod.outboxNOInBase = nod.outboxNOInBase.Append(new DanglingEdge(parrentId, predicate));
                                danglingLinks.Add(new DanglingLink(nod, parrentId, predicate));
                                notFoundedIds.Add(parrentId); 
                            }
                        }
                    }
                }
                
            }

            //3) обходим граф, каждый раз ++, когда из основного цикла уходим в DFS
            List<Dictionary<String, Node>> components = new List<Dictionary<String, Node>>();//хранить компоненты связности
            foreach (KeyValuePair<String, Node> kvp in nodes)
            {
                Node nod = kvp.Value;
                if (nod.flag == false)
                {
                    Dictionary<String, Node> component = new Dictionary<String, Node>();
                    DFS(nodes, nod, component);
                    components.Add(component);
                }
            }
            
            Console.WriteLine("Всего xmls = " + numberOfElementsInDatabase);
            Console.WriteLine("Всего компонент связности " + components.Count);
            int sum = 0;
            foreach (var component in components)
            {
                sum += component.Count;
                Console.WriteLine(component.Count);
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("Сумма элементов из всех компонент связности " + sum);
            Console.WriteLine("Кол-во висячих ссылок " + danglingLinks.Count);

            notFoundedIds = notFoundedIds.Distinct().ToList();
            Console.WriteLine("Кол-во уникальных отсутствующих в базе id " + notFoundedIds.Count() + ". Список:");
            foreach (String id in notFoundedIds)
            {
                Console.WriteLine(id);
            }

            /*
            Func<String, XElement> idSearch = str => dataBase.Elements()
              .FirstOrDefault(elm => elm.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value == str);

            Console.WriteLine(idSearch("syp2007_col_portret32007_pd_097"));
            */
        }
    }
}
