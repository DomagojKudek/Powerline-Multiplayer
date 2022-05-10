using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionDetection : MonoBehaviour
{

    public MapGenerator mapGenerator;
    List<Collider> colliders = new List<Collider>();
    private void Start()
    {

        if (mapGenerator == null)
        {
            //mapGenerator = GetComponentInParent<MapGenerator>();
            mapGenerator = transform.parent.parent.parent.parent.GetComponent<MapGenerator>();
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (mapGenerator != null)
        {
            if ((collision.transform.tag == "winPath") && ((collision.transform.name == "Wire") || (collision.transform.name == "Wire (1)") || (collision.transform.name == "Wire (2)") || (collision.transform.name == "Wire (3)") || (collision.transform.name == "Start(Clone)") || (collision.transform.name == "Finish(Clone)")))
            {
                int location_x = (int)this.gameObject.transform.parent.position.x;
                int location_z = (int)this.gameObject.transform.parent.position.z;
                //Debug.Log("block.world"+location_x + " " + location_z,this.gameObject);

                for (int i = 0; i < mapGenerator.path.Count; i++)
                {
                    //Debug.Log("winpath.world"+mapGenerator.path[i].worldPosition.x + " " + mapGenerator.path[i].worldPosition.z, this.gameObject);
                    if (Mathf.Abs(mapGenerator.path[i].worldPosition.x - location_x) <= 0.5 && Mathf.Abs(mapGenerator.path[i].worldPosition.z - location_z) <= 0.5)
                    {
                        //mapGenerator.winMap[i] = collision.contactCount;// collision.contactCount;
                        foreach (ContactPoint contact in collision.contacts)
                        {
                            if (!colliders.Contains(contact.otherCollider))
                            {
                                mapGenerator.winMap[i] += 1;
                                colliders.Add(contact.otherCollider);
                            }



                            //Debug.Log(contact.thisCollider.name,this);
                            //Debug.Log(" hit " + contact.otherCollider.name, collision.gameObject);
                            // Visualize the contact point
                            // Debug.DrawRay(contact.point, contact.normal, Color.white);
                        }
                        //Debug.Log(mapGenerator.winMap[i] + " " + i, this);
                        //Debug.Log("WINPATHPOS==BLOCKPOS" + i);

                    }
                }

            }

        }
        /*string ispis;
        for(int x=0; x < mapGenerator.setWinPath.Count; x++)
        {

            ispis += 
        }*/

    }

    public void OnCollisionExit(Collision collision)
    {

        if (mapGenerator != null)
        {
            if ((collision.transform.tag == "winPath") && ((collision.transform.name == "Wire") || (collision.transform.name == "Wire (1)") || (collision.transform.name == "Wire (2)") || (collision.transform.name == "Wire (3)") ||
                (collision.transform.name == "Start(Clone)") || (collision.transform.name == "Finish(Clone)")))
            {
                int location_x = (int)this.gameObject.transform.parent.position.x;
                int location_z = (int)this.gameObject.transform.parent.position.z;
                //Debug.Log("block.world"+location_x + " " + location_z,this.gameObject);

                for (int i = 0; i < mapGenerator.path.Count; i++)
                {
                    //Debug.Log("winpath.world"+mapGenerator.path[i].worldPosition.x + " " + mapGenerator.path[i].worldPosition.z, this.gameObject);
                    if (Mathf.Abs(mapGenerator.path[i].worldPosition.x - location_x) <= 0.5 && Mathf.Abs(mapGenerator.path[i].worldPosition.z - location_z) <= 0.5)
                    {

                        mapGenerator.winMap[i] -= 1;


                        //Debug.Log(mapGenerator.winMap[i] + " " + i);
                        //Debug.Log("WINPATHPOS==BLOCKPOS" + i);

                    }
                }

            }

        }
        colliders.Clear();
    }

    IEnumerator waiter()
    {
        yield return new WaitForSecondsRealtime(5);
    }
}
