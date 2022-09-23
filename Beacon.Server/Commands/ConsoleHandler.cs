using System.Text.RegularExpressions;

namespace Beacon.Server.CLI;

internal class ConsoleHandler
{
    private const string _inputRegex = ".*";
    private const int MaxHistory = 25;
    private readonly AutoCompleteNode _root;
    private readonly LinkedList<string> _history;

    public ConsoleHandler(AutoCompleteNode root)
    {
        _root = root;
        _history = new LinkedList<string>();
        HintColor = ConsoleColor.DarkGray;
        OkColor = ConsoleColor.Gray;
    }

    public ConsoleColor HintColor { get; set; }
    public ConsoleColor OkColor { get; set; }

    public event EventHandler<string> EnteredCommand;

    public Task Handle(CancellationToken ctoken)
    {
        while (!ctoken.IsCancellationRequested)
        {
            var cmd = ReadHintedLine();
            if (!string.IsNullOrWhiteSpace(cmd))
                EnteredCommand?.Invoke(this, cmd);
        }

        return Task.CompletedTask;
    }

    private string ReadHintedLine(ConsoleColor hintColor = ConsoleColor.DarkGray)
    {
        ConsoleKeyInfo input;

        string? suggestion = null;
        var userInput = string.Empty;
        var readLine = string.Empty;
        var searchIndex = 0;

        while (ConsoleKey.Enter != (input = Console.ReadKey(true)).Key)
        {
            var cursorPos = Console.CursorLeft;

            switch (input.Key)
            {
                case ConsoleKey.Backspace:
                    if (userInput.Length == 0) continue;
                    userInput = userInput.Remove(--cursorPos, 1);
                    break;

                case ConsoleKey.Tab:
                    if (suggestion is null)
                        continue;

                    userInput = userInput + suggestion ?? userInput;
                    cursorPos = userInput.Length;
                    break;

                case ConsoleKey.UpArrow:
                    if (searchIndex < _history.Count)
                    {
                        userInput = _history.ElementAt(searchIndex++);
                        cursorPos = userInput.Length;
                    }

                    break;

                case ConsoleKey.DownArrow:
                    userInput = searchIndex == 0 ? string.Empty : _history.ElementAt(--searchIndex);
                    cursorPos = userInput.Length;
                    break;

                case ConsoleKey.LeftArrow:
                    if (Console.CursorLeft > 0)
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    continue;

                case ConsoleKey.RightArrow:
                    if (Console.CursorLeft < Console.WindowWidth && Console.CursorLeft < userInput.Length)
                        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                    continue;

                default:
                    if (Regex.IsMatch(input.KeyChar.ToString(), _inputRegex))
                        userInput = userInput.Insert(cursorPos++, input.KeyChar.ToString());

                    break;
            }

            var correctInput = _root.Hint(userInput, out suggestion);
            readLine = suggestion == null ? userInput : userInput + suggestion;

            ClearCurrentConsoleLine();

            var originalColor = Console.ForegroundColor;
            if (correctInput)
                Console.ForegroundColor = OkColor;

            Console.Write(userInput);
            Console.ForegroundColor = hintColor;

            if (userInput.Any())
            {
                Console.Write(readLine.Substring(userInput.Length, readLine.Length - userInput.Length));
                Console.SetCursorPosition(cursorPos, Console.CursorTop);
            }

            Console.ForegroundColor = originalColor;
        }

        if (readLine.Length != 0)
        {
            AddToHistory(readLine);
            Console.WriteLine();
        }

        return userInput.Any() ? readLine : string.Empty;
    }

    private void AddToHistory(string readLine)
    {
        if (_history.Count >= MaxHistory)
            _history.RemoveLast();

        _history.AddFirst(readLine);
    }

    private static void ClearCurrentConsoleLine()
    {
        var currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }
}