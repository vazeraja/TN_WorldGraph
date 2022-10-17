using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feel;
using ThunderNut.WorldGraph;
using UnityEngine;

public class Test : MonoBehaviour {
    public Snake snakeController;

    private WorldGraph worldGraph;

    private void Update() {
        WorldGraphManager.worldGraph.SetInt("_PointsCounter", snakeController.SnakePoints);
    }
}
