using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2

{

    class ProductConsumerQueue
    {
        public Queue<int> queue = new Queue<int>();
        int size;
        public AutoResetEvent qfull = new AutoResetEvent(false);
        public AutoResetEvent qempty = new AutoResetEvent(true);
        

        public ProductConsumerQueue(int s)
        {
            size = s;
        }

        public bool full()
        {
            if (queue.Count == size)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool empty()
        {
            if (queue.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
              
        }

    }
    class Program
    {
        static Mutex mtx = new Mutex();
        static ProductConsumerQueue q = new ProductConsumerQueue(10);
        static int result=0;
        static void produce(List<int> v1, List<int> v2)
        {
            int e;
            int i;
            Random rand = new Random();
            for (i = 0; i < v1.Count; i++)
            {
                e = v1[i] * v2[i];

                Thread.Sleep(rand.Next()%1000);
                mtx.WaitOne();
                while (q.full())
                {
                    mtx.ReleaseMutex();    //1
                    q.qfull.WaitOne();     //2 , 1 and 2 happens at the same time 
                    mtx.WaitOne();
                }
                q.queue.Enqueue(e);
                Console.WriteLine("Sent: " + e);
                q.qempty.Set();
                mtx.ReleaseMutex();
            }
        }

        static void consume(int cnt)
        {
            int i;
            Random rand = new Random();
            for (i = 0; i < cnt; i++)
            {
                Thread.Sleep(rand.Next() % 1000);
                mtx.WaitOne();
                while (q.empty())
                {
                    mtx.ReleaseMutex(); 
                    q.qempty.WaitOne();
                    mtx.WaitOne();
                }
                int a = q.queue.Dequeue();
                Console.WriteLine("Got: " + a);
                result = result + a;
                q.qfull.Set();
                mtx.ReleaseMutex();
            }
        }

        static void Main(string[] args)
        {

            Random rnd = new Random();
            List<int> v1 = new List<int>();
            List<int> v2 = new List<int>();
            int n = 100;

            for (int i = 0; i < n; i++)
            {
                v1.Add(1);
                v2.Add(i+1);
            }


            string v1String = "[" + string.Join(", ", v1) + "]";
            Console.WriteLine("First vector: ");
            Console.WriteLine(v1String);

            string v2String = "[" + string.Join(", ", v2) + "]";
            Console.WriteLine("Second vector: ");
            Console.WriteLine(v2String);
            

            Thread producer_thread = new Thread(() => produce(v1,v2));
            producer_thread.Start();

            Thread consumer_thread = new Thread(() => consume(n));
            consumer_thread.Start();

            producer_thread.Join();
            consumer_thread.Join();


            Console.WriteLine("The result is :" + result);

            Console.ReadLine();

        }
    }

}
