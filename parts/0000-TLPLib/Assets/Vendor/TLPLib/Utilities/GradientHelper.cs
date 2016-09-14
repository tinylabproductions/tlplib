using System;
using UnityEngine;
using System.Collections.Generic;

public static class GradientHelper {

  public static void modifyVertices(List<UIVertex> vertexList, Func<Color32, float, Color32> f) {
    int count = vertexList.Count;
    if (count == 0) return;
    float bottomY = vertexList[0].position.y;
    float topY = vertexList[0].position.y;

    for (int i = 1; i < count; i++) {
      float y = vertexList[i].position.y;
      if (y > topY) {
        topY = y;
      } else if (y < bottomY) {
        bottomY = y;
      }
    }

    float uiElementHeight = topY - bottomY;

    for (int i = 0; i < count; i++) {
      UIVertex uiVertex = vertexList[i];
      uiVertex.color = f(uiVertex.color, (uiVertex.position.y - bottomY) / uiElementHeight);
      vertexList[i] = uiVertex;
    }
  }
}