using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feel;
using ThunderNut.WorldGraph;
using UnityEngine;

public class Test : MonoBehaviour {
    public Snake snakeController;

    private WorldGraphController worldGraphController;
    private void Awake() {
        worldGraphController = WorldGraph.GetWorldGraph().Controller;
    }

    private void Update() {
        worldGraphController.SetInt("_IntParameter", snakeController.SnakePoints);
    }
}
