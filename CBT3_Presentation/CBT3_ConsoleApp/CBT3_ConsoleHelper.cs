//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author: Scott Green
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CBT3_ConsoleApp;

public static class CBT3_ConsoleHelper
{
    public static int width = Console.WindowWidth;
    public static int height = Console.WindowHeight;
    public static int promptWidth = 30; // Adjust this value according to the length of your prompt
    public static int promptHeight = 1; // Adjust this value according to the height of your prompt

    public static int leftPadding = (width - promptWidth) / 2;
    public static int topPadding = (height - promptHeight) / 2;

    public static void EraseLine()
    {
        //int currentLineCursor = Console.CursorTop;
        //Console.SetCursorPosition(0, Console.CursorTop);
        //Console.Write(new string(' ', Console.WindowWidth));
        //Console.SetCursorPosition(0, currentLineCursor - 1);
        Console.Clear();
        Console.WriteLine(new string(' ', Console.WindowWidth));
    }
}
