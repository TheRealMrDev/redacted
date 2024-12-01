using UnityEngine;

public class TodoListSlider : MonoBehaviour
{
    public float slideSpeed = 5f;
    public float slideDistance = 500f;
    public RectTransform todoListPanel;

    private bool isVisible = false;
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;

    void Start()
    {
        visiblePosition = todoListPanel.anchoredPosition;
        hiddenPosition = new Vector2(
            visiblePosition.x + slideDistance,
            visiblePosition.y
        );
    
        todoListPanel.anchoredPosition = hiddenPosition;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleTodoList();
        }

        todoListPanel.anchoredPosition = Vector2.Lerp(
            todoListPanel.anchoredPosition, 
            isVisible ? visiblePosition : hiddenPosition, 
            slideSpeed * Time.deltaTime
        );
    }

    void ToggleTodoList()
    {
        isVisible = !isVisible;
    }
}