using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Sharpen.Lang;

namespace Gamlor.Db4oExt.Tests.IO
{
    public class TaskAssumptions
    {
        [Test]
        public void Tats()
        {
            var task = new Task(() => { Console.Out.WriteLine("Hie"); });
            task.Start();
            task.ContinueWith(t => { Console.Out.WriteLine("Continue"); });
            task.Wait();
            Console.Out.WriteLine("Waitedf on");
            task.ContinueWith(t => { Console.Out.WriteLine("Continue 2"); });
            task.ContinueWith(t => { Console.Out.WriteLine("Continue 3"); });
            task.ContinueWith(t => { Console.Out.WriteLine("Continue 4"); });
            task.ContinueWith(t => { Console.Out.WriteLine("Continue 5"); });
            task.ContinueWith(t =>
                                  {
                                      Console.Out.WriteLine("Continue 2");
                                  });
            Thread.Sleep(1000);
        }
    }
}