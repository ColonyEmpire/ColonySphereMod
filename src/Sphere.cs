using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Permissions;

namespace ExampleMod
{
  [ModLoader.ModManager]
  public class SphereChatModEntries
  {
    [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.sphere.registercommand")]
    public static void AfterItemTypesServer()
    {
      ChatCommands.CommandManager.RegisterCommand(new SphereChatCommand());
    }

    class SphereChatCommand : ChatCommands.IChatCommand
    {
      public bool IsCommand(string chat)
      {
        return chat.StartsWith("/sphere ");
      }

      public bool TryDoCommand(Players.Player causedBy, string chattext)
      {
        if (causedBy != null) {
          foreach (string permission in new string[] { "emperor", "coder", "godemperor", "god" }) {
            if (PermissionsManager.CheckAndWarnPermission(causedBy, permission)) {
              var m = Regex.Match(chattext, @"/sphere (?<px>-?\d+) (?<py>-?\d+) (?<pz>-?\d+) (?<r>\d+) (?<mat>.+)");
              if (m.Success) {
                int px = Int32.Parse(m.Groups["px"].Value);
                int py = Int32.Parse(m.Groups["py"].Value);
                int pz = Int32.Parse(m.Groups["pz"].Value);
                int r = Int32.Parse(m.Groups["r"].Value);
                string mat = m.Groups["mat"].Value;
                if (mat.Equals("water")) {
                  Chat.Send(causedBy, "just don't ...");
                  return true;
                }
                Vector3Int position = new Vector3Int(px, py, pz);
                Chat.Send(causedBy, string.Format("Generating sphere centered at {0} with radius {1}", position, r));
                ushort typeIndex = ItemTypes.IndexLookup.GetIndex(mat);
                ThreadManager.InvokeOnMainThread(delegate ()
                {
                  for (int x = -r + 1; x < r; x++) {
                    for (int y = -r + 1; y < r; y++) {
                      for (int z = -r + 1; z < r; z++) {
                        Vector3Int p = new Vector3Int(px, py, pz).Add(x, y, z);
                        int d = (int) System.Math.Round(System.Math.Sqrt(x * x + y * y + z * z));
                        if (d < r) {
                          ServerManager.TryChangeBlock(p, typeIndex);
                        }
                      }
                    }
                  }
                }, 1.0);
              } else {
                Chat.Send(causedBy, "Command didn't match, use /sphere x y z r materialname");
              }
              return true;
            }
          }
          Chat.Send(causedBy, "Permission denied!");
        }
        return true;
      }
    }
  }
}
