using UnityEngine;
namespace ProjectMeadow.Items
{
    public sealed class MilkCanVisual : MonoBehaviour
    {
        private Mesh mesh;
        public void Build(Material metal, Material accent)
        {
            // Original turned can profile: foot, body, shoulder, neck and lid.
            Vector2[] profile = {
                new Vector2(.17f, 0), new Vector2(.22f, .03f), new Vector2(.22f, .36f),
                new Vector2(.2f, .42f), new Vector2(.125f, .49f), new Vector2(.125f, .56f),
                new Vector2(.15f, .57f), new Vector2(.15f, .61f), new Vector2(.11f, .63f)
            };
            const int sides = 16;
            var vertices = new Vector3[profile.Length * (sides + 1) + 2];
            var triangles = new System.Collections.Generic.List<int>();
            for (int row = 0; row < profile.Length; row++)
            {
                for (int side = 0; side <= sides; side++)
                {
                    float angle = side * Mathf.PI * 2 / sides;
                    vertices[row * (sides + 1) + side] = new Vector3(Mathf.Cos(angle) * profile[row].x, profile[row].y, Mathf.Sin(angle) * profile[row].x);
                    if (row == 0 || side == sides) continue;
                    int a = (row - 1) * (sides + 1) + side, b = a + sides + 1;
                    triangles.Add(a); triangles.Add(b); triangles.Add(a + 1);
                    triangles.Add(a + 1); triangles.Add(b); triangles.Add(b + 1);
                }
            }
            int bottom = vertices.Length - 2, top = vertices.Length - 1;
            vertices[bottom] = Vector3.zero; vertices[top] = Vector3.up * profile[profile.Length - 1].y;
            for (int side = 0; side < sides; side++)
            {
                triangles.Add(bottom); triangles.Add(side); triangles.Add(side + 1);
                int last = (profile.Length - 1) * (sides + 1) + side;
                triangles.Add(top); triangles.Add(last + 1); triangles.Add(last);
            }
            mesh = new Mesh { name = "Original milk can" };
            mesh.vertices = vertices; mesh.triangles = triangles.ToArray(); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = metal;
            var hitbox = gameObject.AddComponent<CapsuleCollider>();
            hitbox.center = new Vector3(0, .32f, 0); hitbox.radius = .23f; hitbox.height = .64f;
            var handle = new GameObject("Arched handle"); handle.transform.SetParent(transform, false);
            var line = handle.AddComponent<LineRenderer>();
            line.useWorldSpace = false; line.positionCount = 13; line.widthMultiplier = .035f;
            line.numCapVertices = 3; line.numCornerVertices = 2; line.generateLightingData = true;
            line.sharedMaterial = metal;
            for (int i = 0; i < 13; i++)
            {
                float angle = Mathf.PI * i / 12;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * .21f, .39f + Mathf.Sin(angle) * .36f, 0));
            }
            var band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            band.name = "Teal milk band"; band.transform.SetParent(transform, false);
            band.transform.localPosition = new Vector3(0, .23f, 0);
            band.transform.localScale = new Vector3(.446f, .025f, .446f);
            band.GetComponent<Renderer>().sharedMaterial = accent;
            var collider = band.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
        }
        private void OnDestroy() { if (mesh != null) Destroy(mesh); }
    }
}
