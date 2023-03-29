// See https://aka.ms/new-console-template for more information
using AtomReaderNet;

using var reader = new AtomReader("Hello world\nHello moon");
while (!reader.EndOfStream)
{
    Console.WriteLine(new AtomString(reader.ReadLine()));
}
