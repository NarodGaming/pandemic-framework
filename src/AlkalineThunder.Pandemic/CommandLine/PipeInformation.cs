using System.IO;

namespace AlkalineThunder.Pandemic.CommandLine
{
    public struct PipeInformation
    {
        public string[] Tokens;
        public StreamReader StdIn;
        public StreamWriter StdOut;

        public PipeInformation(string[] tokens, StreamReader input, StreamWriter output)
        {
            Tokens = tokens;
            StdIn = input;
            StdOut = output;
        }

    }
}