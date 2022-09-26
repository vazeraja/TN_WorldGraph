using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.WorldGraph.Editor {

    public class GraphInspectorView : VisualElement {
        private readonly WorldGraph m_Graph;
        private WSGGraphView graphView;
        private EditorWindow editorWindow;

        public Label m_Title;

        public VisualElement content { get; set; }
        public Image previewTextureView { get; set; }
        private Vector2 m_PreviewScrollPosition;

        private ResizeBorderFrame m_PreviewResizeBorderFrame;
        public ResizeBorderFrame previewResizeBorderFrame => m_PreviewResizeBorderFrame;

        public GraphInspectorView() {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/GraphInspectorView"));

            var topContainer = new VisualElement() {name = "top"};
            {
                m_Title = new Label {name = "title", text = "Graph Inspector"};
                topContainer.Add(m_Title);
            }
            Add(topContainer);

            content = new VisualElement {name = "middle"};
            {
                previewTextureView = CreatePreview(Texture2D.redTexture);
                m_PreviewScrollPosition = new Vector2(0f, 0f);
                content.Add(previewTextureView);

                content.AddManipulator(new Scrollable((x) => { }));
            }
            Add(content);

            m_PreviewResizeBorderFrame = new ResizeBorderFrame(this, this) {name = "resizeBorderFrame", maintainAspectRatio = true};
            Add(m_PreviewResizeBorderFrame);
        }


        private Image CreatePreview(Texture2D texture) {
            var image = new Image {name = "content", image = texture, scaleMode = ScaleMode.ScaleAndCrop};
            return image;
        }
    }

}