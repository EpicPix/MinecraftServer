namespace MinecraftServer;

public class ChatComponent
{
    private string _text;

    public string text => _text; // dont change name for correct serialization

    public ChatComponent(string text)
    {
        _text = text;
    }

    public ChatComponent SetText(string text)
    {
        _text = text;
        return this;
    } 
}