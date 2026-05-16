using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace StateMachineTool.Editor
{
    public class StateNodeView : Node
    {
        public string StateId { get; }
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }

        public StateNodeView(string stateId, string titleText, Runtime.StateType stateType, Vector2 position)
        {
            StateId = stateId;
            viewDataKey = stateId;
            title = titleText;

            SetPosition(new Rect(position, new Vector2(180, 0)));
            capabilities |= Capabilities.Movable | Capabilities.Selectable | Capabilities.Deletable;

            // Entry states have no input port
            if (stateType != Runtime.StateType.Entry)
            {
                InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
                InputPort.portName = "In";
                InputPort.name = "in";
                inputContainer.Add(InputPort);
            }

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            OutputPort.portName = "Out";
            OutputPort.name = "out";
            outputContainer.Add(OutputPort);

            // Type badge in title bar
            var badge = new Label(stateType.ToString());
            badge.style.fontSize = 10;
            badge.style.unityTextAlign = TextAnchor.MiddleCenter;
            badge.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            badge.style.paddingTop = 1; badge.style.paddingBottom = 1;
            badge.style.paddingLeft = 6; badge.style.paddingRight = 6;
            badge.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
            badge.style.borderTopLeftRadius = 3; badge.style.borderTopRightRadius = 3;
            badge.style.borderBottomLeftRadius = 3; badge.style.borderBottomRightRadius = 3;
            titleButtonContainer.Add(badge);

            ApplyStateStyle(stateType);
        }

        public void ApplyStateStyle(Runtime.StateType stateType)
        {
            RemoveFromClassList("entry");
            RemoveFromClassList("any");
            RemoveFromClassList("normal");

            switch (stateType)
            {
                case Runtime.StateType.Entry:
                    AddToClassList("entry");
                    titleContainer.style.backgroundColor = new Color(0.16f, 0.37f, 0.2f, 1f);
                    titleContainer.style.color = Color.white;
                    break;
                case Runtime.StateType.Any:
                    AddToClassList("any");
                    titleContainer.style.backgroundColor = new Color(0.37f, 0.27f, 0.11f, 1f);
                    titleContainer.style.color = Color.white;
                    break;
                default:
                    AddToClassList("normal");
                    titleContainer.style.backgroundColor = new Color(0.22f, 0.24f, 0.31f, 1f);
                    titleContainer.style.color = new Color(0.85f, 0.85f, 0.9f, 1f);
                    break;
            }
        }

        public void SetTitle(string newTitle) => title = newTitle;
    }
}
