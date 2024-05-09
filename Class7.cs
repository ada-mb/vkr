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
        public static void DFS(List<Node> nodes, Node nod, List<Node> component)
        {
            nod.flag = true;
            component.Add(nod);
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
            XElement dataBase = XElement.Load(@"C:\Users\admat\source\repos\с#\PracticeCSharp\SypCassete_current.fog");
            //XElement dataBase = XElement.Load(@"C:\Users\admat\source\repos\с#\PracticeCSharp\test2.fog");
            IEnumerable<XElement> elements = dataBase.Elements(); //dataBase.Elements() - получили поток 
            int numberOfElementsInDatabase = elements.Count();

            List<Node> nodes = new List<Node>(numberOfElementsInDatabase);
            //преимущество листа - встроенная функция find и возможность заранее указать емкость
            //тем самым делая операцию добавления за o(1) + он тоже IEnumerable
            //https://www.claudiobernasconi.ch/2013/07/22/when-to-use-ienumerable-icollection-ilist-and-list/

            //1) создаем для каждого xml свой node
            foreach (XElement xml in elements)
            {
                String id = xml.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value;
                Node nod = new Node(id, xml);
                nodes.Add(nod);
            }


            //2) идем по nodes и соединяем их с другими nodes
            //побочным действием алгоритма явл выявление висящих ссылок во всей базе, которые мы будем хранить в danglingLinks
            List<DanglingLink> danglingLinks = new List<DanglingLink>(); 
            foreach (Node nod in nodes)
            {
                foreach (var field in nod.xml.Elements())
                {
                    var resource = field.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}resource");
                    if (resource != null)
                    {
                        XName predicate = field.Name; 
                        String parrentId = resource.Value;
                        {
                            Node parrentNod = nodes.Find(x => x.id == parrentId);
                            if (parrentNod != null)
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
                            }
                        }
                    }
                }
                
            }

            //3) обходим граф, каждый раз ++, когда из основного цикла уходим в DFS
            List<List<Node>> components = new List<List<Node>>();//хранить компоненты связности
            foreach (Node nod in nodes)
            {
                if (nod.flag == false)
                {
                    List<Node> component = new List<Node>();
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
            }
            Console.WriteLine();
            Console.WriteLine("Сумма элементов из всех компонент связности " + sum);
            Console.WriteLine("Кол-во висячих ссылок " + danglingLinks.Count);

        }
    }
}