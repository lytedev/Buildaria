using System;

namespace Buildaria
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Core game = new Core())
            {
                game.Run();
            }
        }
    }
#endif
}

