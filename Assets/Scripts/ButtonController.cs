using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  public Vector2Int hexCoords;
  public TileType type;
  public UnityEvent onMouseEnter = new UnityEvent();
  public UnityEvent onMouseExit = new UnityEvent();
  public Color hoverTextColor = Color.green;
  public Color disabledHoverTextColor = Color.red;
  Color originalTextColor;
  bool isHovering = false;

  Text label;
  Button button;
  ScoreKeeper scoreKeeper;
  TileGrid grid;
  TileTypeData data => TileTypeData.Get(type);

  bool isEnabled =>
    scoreKeeper.points >= data.cost &&
    isVisible;

  bool _isVisible = false;
  bool isVisible =>
    grid.CanBuild(hexCoords, type) &&
    (grid.GetTile(hexCoords) == null || !grid.GetTile(hexCoords).isBuilding);

  void Start()
  {
    button = GetComponent<Button>();
    label = GetComponentInChildren<Text>();
    scoreKeeper = transform.root.GetComponentInChildren<ScoreKeeper>();
    grid = GetComponentInParent<TileGrid>();
    originalTextColor = label.color;
  }

  void Update()
  {
    if (isHovering && !_isVisible && isVisible)
    {
      onMouseEnter.Invoke();
    }
    else if (isHovering && _isVisible && !isVisible)
    {
      onMouseExit.Invoke();
    }
    _isVisible = isVisible;

    label.gameObject.SetActive(isVisible);
    label.text = data.label;
    button.interactable = isEnabled;
    label.color = isHovering
      ? (isEnabled ? hoverTextColor : disabledHoverTextColor)
      : originalTextColor;
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (isVisible)
    {
      onMouseEnter.Invoke();
      isHovering = true;
    }
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    if (isVisible)
    {
      onMouseExit.Invoke();
      isHovering = false;
    }
  }
}
