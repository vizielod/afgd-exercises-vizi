#define SOLUTION_3_1
#define SOLUTION_3_2
#define SOLUTION_3_3

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AfGD.Assignment3
{
    class Node
    {
        enum Axis
        {
            X = 0,
            Z = 1,
        }

        // Children of this node
        Node m_ChildA;
        Node m_ChildB;

        // Axis along which this cell is split
        Axis m_SplitAxis;

        // Volume owned by this node.
        Bounds m_Cell;

        // Volume of the dungeon room(s) inside this node.
        // If this is a leaf node it will match the volume of the room exactly.
        // Otherwise these bounds will encapsulate all rooms inside this cell.
        Bounds m_Room;

        // Color of this node.
        // Used for debug purposes.
        Color m_Color;

        public bool IsConnected { get; private set; }
        public bool IsLeafNode => m_ChildA == null || m_ChildB == null;

        public Bounds Room => m_Room;

        public Node(Bounds data)
        {
            m_Cell = data;
            m_Color = Color.HSVToRGB(Random.value, 0.8f, 1f);
        }

#if SOLUTION_3_1
        // Returns a boolean whether a cell is considered valid
        // Valid cells may have constraints on values
        // This may include but is not limited to volume and/or ratio
        bool IsValidCell()
        {
            float volume = m_Cell.size.x * m_Cell.size.y * m_Cell.size.z;
            if (volume < 20f * 2f * 20f) // you can play with these parameters
                return false;

            float ratio = Mathf.Abs(m_Cell.size.x / m_Cell.size.z);
            if (ratio > 5f || ratio < .2f) // you can play with these parameters
                return false;

            return true;
        }
#endif

        public void SplitCellRecursively()
        {
#if SOLUTION_3_1
            // Do not attempt to split an invalid cell
            if (!IsValidCell())
                return;
#endif

            // Split this cell if it is valid and does not have children
            if (m_ChildA == null && m_ChildA == null)
                SplitCell();

            // Split its children
            m_ChildA?.SplitCellRecursively();
            m_ChildB?.SplitCellRecursively();
        }

        // Splits a cell into two smaller cells along a random axis
        // Only if the newly formed cells are both valid
        // TODO Assignment 3.1 - Attempt to split this cell into two partitions
        // 1) split this cell along a random axis.
        // 2) check if the newly created partions are valid paritions
        // 3) only if both partions are valid assign them as new child nodes
        //    of this node (m_ChildA & m_ChildB)
        void SplitCell()
        {
#if SOLUTION_3_1
            // Randomly choose an axis to split on
            /*int rand = Random.Range(0, 2);
            Debug.Log("Random number: " + rand);
            if (rand == 0) m_SplitAxis = Axis.X;
            else m_SplitAxis = Axis.Z;*/
            m_SplitAxis = (Axis)Random.Range(0, 2);
            //Debug.Log("Random axis: " + m_SplitAxis);

            // Partition the Cell into two cells; cellA & cellB.
            var min = m_Cell.min;
            var max = m_Cell.max;
            var length = (m_SplitAxis == Axis.X) ? max.x - min.x : max.z - min.z;
            //Debug.Log("Min: " + min + " Max: " + max + " Length: " + length);
            var randomPointOnAxis = (m_SplitAxis == Axis.X) ? Random.Range(min.x + .2f * length, max.x - .2f * length) : Random.Range(min.z + .2f * length, max.z - .2f * length);
            //Debug.Log("Random Point On Axis: " + randomPointOnAxis);
            if(m_SplitAxis == Axis.X)
            {
                max.x = randomPointOnAxis;
            }
            else
            {
                max.z = randomPointOnAxis;
            }

            var cellA = new Node(
                new Bounds
                {
                    min = min,
                    max = max
                });

            min = m_Cell.min;
            max = m_Cell.max;
            if (m_SplitAxis == Axis.X)
            {
                min.x = randomPointOnAxis;
            }
            else
            {
                min.z = randomPointOnAxis;
            }

            var cellB = new Node(
                new Bounds
                {
                    min = min,
                    max = max
                });
            // Only if we can split into two valid cells do we actually proceed with the split
            // m_ChildA = ...;
            // m_ChildB = ...;
            if (cellA.IsValidCell() && cellB.IsValidCell())
            {
                m_ChildA = cellA;
                m_ChildB = cellB;
            }
#endif

        }

        public void GenerateRoomsRecursively()
        {
            m_ChildA?.GenerateRoomsRecursively();
            m_ChildB?.GenerateRoomsRecursively();

            if (IsLeafNode)
                GenerateRoom();
        }

        // Randomly generates a room within the bounds of this partition.
        // This function is only called on leaf nodes.
        // TODO Assignment 3.2 - Generate a room in leaf nodes
        // 1) Create a room within the volume of its partion (m_Cell)
        // 2) Add any needed constraints to ensure decent looking rooms.
        //      tip: make a room occupy at least half of the x and z axis of the parition. 
        //           this will make things easier in assignment 3.3
        // 3) Assign the volume of the room to m_Room.
        void GenerateRoom()
        {
#if SOLUTION_3_2
            // Randomly generate bounds for the room
            var roomSize_x = Random.Range(Mathf.Min(m_Cell.size.x * .5f + 1f, m_Cell.size.x - 5f), m_Cell.size.x - 5f);
            var roomSize_y = m_Cell.size.y;
            var roomSize_z = Random.Range(Mathf.Min(m_Cell.size.z * .5f + 1f, m_Cell.size.z - 5f), m_Cell.size.z - 5f);

            var x = Random.Range(0f, m_Cell.size.x - roomSize_x);
            var z = Random.Range(0f, m_Cell.size.z - roomSize_z);

            var min = new Vector3(x, 0f, z);
            var max = new Vector3(x + roomSize_x, roomSize_y, z + roomSize_z);

            min += m_Cell.min;
            max += m_Cell.min;

            // m_Room = ... (todo)
            m_Room = new Bounds { min = min, max = max };
#endif

            // Spawn Mesh that represents room
            GameObject room = GameObject.CreatePrimitive(PrimitiveType.Cube);
            room.transform.position = m_Room.center;
            room.transform.localScale = m_Room.size;
            room.GetComponent<MeshRenderer>().material.color = m_Color;
            room.name = "Room";
        }

        // Recursively create pathways between segments of our dungeon.
        // Returns a boolean if a change was made. 
        public bool ConnectRoomsRecursively()
        {
            // If this node is connected to the dungeon 
            // There is no need to update anything.
            if (IsConnected || IsLeafNode)
                return false;

            bool childUpdated = false;

            // Update Children first
            if (m_ChildA != null)
                childUpdated |= m_ChildA.ConnectRoomsRecursively();

            if (m_ChildB != null)
                childUpdated |= m_ChildB.ConnectRoomsRecursively();

            // If a child has updated, we cannot update this frame.
            if (childUpdated)
                return true;

            ConnectChildRooms();
            IsConnected = true;
            return true;
        }

        // ! This function is called over multiple frames in case you want to make use of Physics.Raycast().
        // !                (You cannot raycast against newly instantiated objects immediately)
        // ! The function will ensure that it is safe to raycast against the rooms & corridors of the child nodes.
        // 
        // TODO Assignment 3.3 - Create hallways between rooms
        // 1) Starting from the lowest layers, connect rooms corresponding to children of the same parent
        //    - If the rooms occupy at least half of both axis inside a partition you should only ever require straight corridors
        //      In other words, there will always be a section of the RoomBounds that can be connected by a straight line.
        //    - Once rooms are connected by a hallway we can connect to that hallways too. 
        //      Therefore the first condition applies for higher levels of the tree hierarchy too!
        //    - Consider using Physics.RayCast or Physics.BoxCast to cast into a RoomBound to find the actual room or hallway it should connect to.
        //    - Spawn a cube (similar to what is done for rooms) once you have found the dimensions and location of the hallway.
        //      Otherwise you do not have anything to raycast against.
        void ConnectChildRooms()
        {
            // Connect m_ChildA & m_ChildB
            // example of algorithm to connect the child nodes:
            // 1) Find interval where the RoomBounds of the children overlap on the plane perpendicular to the split axis of *this* node
            // 2) Sanity check if this range exists (should be the case if a room is always at least half the size)
            // 3) Pick a point on this interval where the corridor will be placed (this is only 1 coordinate)
            // 4) Find a coordinate on the split axis that is inbetween the RoomBounds of both children 
            //    (the second coordinate, now we have a point between the two child nodes we want to connect)
            // 5) RayCast from the found point along the split axis in both directions 
            //    (this should give you the coordinates where the hallway connect to the room 
            //        (or another hallway if the connecting child node has child nodes itself))
            // 6) Create a Cube that represents the hallway (i.e. GameObject.CreatePrimitive(PrimitiveType.Cube);)
#if SOLUTION_3_3
            var leftAABB = m_ChildA.Room;
            var rightAABB = m_ChildB.Room;

            var corridorDirection = m_SplitAxis == Axis.X ? Vector3.forward : Vector3.right;

            var leftRoomRange = new Vector2(
                Vector3.Dot(leftAABB.min, corridorDirection),
                Vector3.Dot(leftAABB.max, corridorDirection)
                );

            //Debug.Log("LeftRange: " + leftRoomRange);

            var rightRoomRange = new Vector2(
                Vector3.Dot(rightAABB.min, corridorDirection),
                Vector3.Dot(rightAABB.max, corridorDirection)
                );

            //Debug.Log("RightRange: " + rightRoomRange);
            
            //1) Find interval where the RoomBounds of the children overlap on the plane perpendicular to the split axis of *this * node
            //The higher value of the mins and the lower value of the maxes
            var overlapRange = new Vector2(
                (leftRoomRange.x <= rightRoomRange.x) ? rightRoomRange.x : leftRoomRange.x,
                (leftRoomRange.y >= rightRoomRange.y) ? rightRoomRange.y : leftRoomRange.y
                );

            //Debug.Log("OverlapRange: " + overlapRange);

            // 2) Sanity check if this range exists (should be the case if a room is always at least half the size)
            if (overlapRange.x < overlapRange.y)
            {
                // 3) Pick a point on this interval where the corridor will be placed (this is only 1 coordinate)
                var overlapPosition = overlapRange.x + Random.value * (overlapRange.y - overlapRange.x);

                var leftPosition = leftAABB.center;
                var rightPosition = rightAABB.center;

                //perpendicular axis
                if(m_SplitAxis == Axis.X)
                {
                    leftPosition.z = overlapPosition;
                    rightPosition.z = overlapPosition;
                }
                else
                {
                    leftPosition.x = overlapPosition;
                    rightPosition.x = overlapPosition;
                }

                // 4) Find a coordinate on the split axis that is inbetween the RoomBounds of both children 
                //    (the second coordinate, now we have a point between the two child nodes we want to connect)
                int axis = (int)m_SplitAxis;
                if (axis == 0)
                {
                    var sign = Mathf.Sign((rightPosition - leftPosition).x);
                    leftPosition.x += sign * leftAABB.extents.x;
                    rightPosition.x += -sign * rightAABB.extents.x;
                }
                else
                {
                    var sign = Mathf.Sign((rightPosition - leftPosition).z);
                    leftPosition.z += sign * leftAABB.extents.z;
                    rightPosition.z += -sign * rightAABB.extents.z;
                }

                // 5) RayCast from the found point along the split axis in both directions 
                //    (this should give you the coordinates where the hallway connect to the room 
                //        (or another hallway if the connecting child node has child nodes itself))
                var rayOrigin = 0.5f * (leftPosition + rightPosition);

                var hallwayDirection = axis == 0 ? Vector3.right : Vector3.forward;

                Physics.Raycast(rayOrigin, hallwayDirection, out var left);
                Physics.Raycast(rayOrigin, -hallwayDirection, out var right);

                var dir = left.point - right.point;

                // Spawn Mesh that represents corridor
                GameObject corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                corridor.transform.position = 0.5f * (left.point + right.point);
                var scale = Vector3.one;
                if(axis == 0)
                {
                    scale.x = Mathf.Abs(dir.x);
                }
                else
                {
                    scale.z = Mathf.Abs(dir.z);
                }
                corridor.transform.localScale = scale;
                corridor.name = "Corridor";
            }

#endif
        }

        public void UpdateRoomBoundsRecursively()
        {
            // Visit children first and update their bounds.
            m_ChildA?.UpdateRoomBoundsRecursively();
            m_ChildB?.UpdateRoomBoundsRecursively();

            // Only our bounds after our children.
            UpdateRoomBounds();
        }

        // Encapsulates the room bounds of the child nodes.
        // If this node is a leaf node the room bounds contain the room exactly.
        // Otherwise it is an AABB around all the nodes in descenting children.
        void UpdateRoomBounds()
        {
            if (m_ChildA != null)
            {
                if (m_Room.Equals(new Bounds()))
                    m_Room = m_ChildA.Room;
                else
                    m_Room.Encapsulate(m_ChildA.Room);
            }

            if (m_ChildB != null)
            {
                if (m_Room.Equals(new Bounds()))
                    m_Room = m_ChildB.Room;
                else
                    m_Room.Encapsulate(m_ChildB.Room);
            }
        }

        public void DebugDraw(DrawMode drawMode)
        {
            Color color = Handles.color;
            Handles.color = m_Color;

            // Select bounds based on drawing mode
            Bounds AABB = drawMode == DrawMode.Rooms ? m_Room : m_Cell;
            Vector3 min = AABB.min;
            Vector3 max = AABB.max;
            max.y = min.y;

            // Draw a cross at the bottom of the volume
            Handles.DrawLine(min, max);
            float x = min.x; min.x = max.x; max.x = x;
            Handles.DrawLine(min, max);

            // Draw the volume
            Handles.DrawWireCube(AABB.center, AABB.size);
            Handles.Label(AABB.center, new GUIContent($"w:{AABB.size.x:N0}, h:{AABB.size.z:N0}, v:{(AABB.size.x * AABB.size.y * AABB.size.z):N0}"));

            Handles.color = color;
        }

        // Retrieves all the leaf nodes of this graph.
        public void GetLeafNodes(List<Node> nodes)
        {
            // If this is a leaf node add it to the result
            if (IsLeafNode)
            {
                nodes.Add(this);
            }
            // Otherwise keep looking
            else
            {
                m_ChildA?.GetLeafNodes(nodes);
                m_ChildB?.GetLeafNodes(nodes);
            }
        }

        // Retrieves all nodes at a certain level of the graph.
        public void GetNodesAtLevel(List<Node> nodes, int level)
        {
            // If this node is the target level add it to the result
            if (level == 0)
            {
                nodes.Add(this);
            }
            // Otherwise keep looking
            else
            {
                m_ChildA?.GetNodesAtLevel(nodes, level - 1);
                m_ChildB?.GetNodesAtLevel(nodes, level - 1);
            }
        }
    }
}