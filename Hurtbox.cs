using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class Hurtbox : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(.2f, .2f, 1f, .25f);
        BoxCollider2D bc2d = GetComponent<BoxCollider2D>();
        Vector3 mult = bc2d.offset;
        mult.Scale(new Vector3(GetComponentInParent<Character>().facingRight ? 1 : -1, 1));
        Gizmos.DrawCube(transform.root.position + mult, bc2d.bounds.extents * 2);
    }
}
