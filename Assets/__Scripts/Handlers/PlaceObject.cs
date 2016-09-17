using UnityEngine;
using System.Collections.Generic;

public class PlaceObject : MonoBehaviour {

    public Camera tpCamera;
    
    private bool _placing = false;
    private bool _spawn = false;
    private float? _snapX;
    private float? _snapZ;
    private int _lastPressed;
    private int _numKey;
    private GameObject _itemInHand;
    private GameObject _snapPoint;
    private Material _originalMaterial;
    private Ray _ray;
    private RaycastHit _hit;

    public List<GameObject> objectList;

    private List<Transform> _allChildren;

    void Start () {
        // Assigns _allChildren as a new list, because Unity
        _allChildren = new List<Transform>();
    }

    void Update () {
        // If the player hits any number button 1-8 above the qwerty line
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4) ||
            Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Alpha6) ||
            Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Alpha8)) {
            // Gets the number the player pressed, parses it to an int, and sets it as _numKey
            _numKey = int.Parse(Input.inputString);
            // If _placing is false...
            if (!_placing) {
                // Prep the spawning
                _placing = true;
                _spawn = true;
            } else {
                // Reset values and destory old item in hand
                _placing = false;
                Destroy(_itemInHand);
                _allChildren.Clear();
                _itemInHand = null;
                // If the new number pressed is different from what was last pressed and not a default value
                if (_numKey != 0 && _lastPressed != _numKey) {
                    // Prep the spawning again
                    _placing = true;
                    _spawn = true;
                }
            }
            // Hold the last button pressed for compairison later
            _lastPressed = int.Parse(Input.inputString);
        }

        // If we're ready to spawn, call the Prelims(), and stop them from being called again
        if (_spawn) {
            Prelims();
            _spawn = false;
        }

        // If _placing is true...
        if (_placing) {
            // Calls Place method
            Place();
        }
    }

    void Prelims () {
        // Determines the object the player wants to spawn and sets _itemInHand as the spawned object
        _itemInHand = (GameObject)Instantiate(objectList[(int)_numKey-1], Vector3.zero, Quaternion.identity);
        // If the item in hand is a mine, randomly rotate each mine on it's y axis
        if(_itemInHand.tag == "Mine") {
            foreach(Transform _mine in _itemInHand.gameObject.GetComponentInChildren<Transform>()) {
                _mine.Rotate(0, Random.Range(0, 359), 0);
            }
        }
        // For each child in _itemInHand, if it has a Box Collider, change it's layer to ignore Raycasts
        foreach(Transform _child in _itemInHand.gameObject.GetComponentsInChildren<Transform>()) {
            _child.gameObject.layer = 2;
            _allChildren.Add(_child);
        }
        // For every child (direct and indirect) in the object check if they have a mesh renderer, save the original material
        // and set the material for the object to it's ghost version
        foreach(Transform _child in _allChildren) {
            if(_child.GetComponent<MeshRenderer>() != null) {
                Material mat = _child.GetComponent<Renderer>().material;
                _originalMaterial = mat;
                mat = _itemInHand.GetComponent<Tower>().ghostMaterial;
                _child.GetComponent<Renderer>().material = mat;
            }
        }
    }

    void Place () {
        // Set _ray to the ray between the main camera and where the mouse pointer is on the screen
        _ray = tpCamera.ScreenPointToRay(Input.mousePosition);
        // If the raycast hits anything in the loaded world...
        if (Physics.Raycast(_ray, out _hit, Mathf.Infinity)) {
            // Move the object being spawned to where the mouse is over
            _itemInHand.transform.position = _hit.point;
            if (_itemInHand.tag == "Tower") {
                // If the hit object has the tag of "Pedestal"...
                if(_hit.collider.tag == "Pedestal") {
                    Snap();
                } else {
                    // Else sets the shorcuts to null
                    _snapPoint = null;
                    _snapX = null;
                    _snapZ = null;
                }
            } else if (_itemInHand.tag == "Mine") {
                if (_hit.collider.tag == "Walkway") {
                    Snap();
                } else {
                    // Else sets the shorcuts to null
                    _snapPoint = null;
                    _snapX = null;
                    _snapZ = null;
                }
            }
        }
    }

    void Snap () {
        // Sets shortcut values
        _snapPoint = _hit.collider.gameObject;
        _snapX = _snapPoint.transform.position.x;
        _snapZ = _snapPoint.transform.position.z;
        // Sets the _itemInHand to "snap" to the top-center of the snap point so it looks like it's on it
        _itemInHand.transform.position = new Vector3((float)_snapX, CalculateTopPosition(_snapPoint), (float)_snapZ);
        // If the player left clicks...
        if (Input.GetMouseButtonDown(0)) {
            // Stop the placing loop
            _placing = false;
            // Change all materials to the original
            foreach (Transform _child in _allChildren) {
                if (_child.GetComponent<MeshRenderer>() != null) {
                    _child.GetComponent<Renderer>().material = _originalMaterial;
                    _child.gameObject.layer = 0;
                }
            }
            // Clear and reset all assigned variables
            _allChildren.Clear();
            _originalMaterial = null;
            _snapPoint = null;
            _snapX = null;
            _snapZ = null;
            _itemInHand = null;
        }
    }

    // Calculates the top-most global y position of any object by finding the center global y position of the object
    // and adding half the height of the object
    float CalculateTopPosition (GameObject _object) {
        float _top = 0;
        
        _top = _snapPoint.GetComponent<Renderer>().bounds.center.y + (_snapPoint.GetComponent<Renderer>().bounds.size.y/2);

        return _top;
    }
}