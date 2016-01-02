using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIButtonUndo : MonoBehaviour {
    private List<GameScene.Command> commands = new List<GameScene.Command>();

    public static UIButtonUndo invoker = null;

    public UIButtonUndo()
    {
       invoker = this;
    }

    public void AddCommand(GameScene.Command command)
    {
        commands.Add(command);
    }

    public void ExecuteCommand()
    {
        commands[commands.Count - 1].Execute();
    }

    public void UnExecuteCommand()
    {
        if (commands.Count >= 1)
        {
            commands[commands.Count - 1].UnExecute();
            commands.RemoveAt(commands.Count - 1);
        }
    }

    public void Clear()
    {
        commands.Clear();
    }
}
