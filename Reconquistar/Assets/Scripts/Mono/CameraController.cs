using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    //카메라 사이즈 변경을 통한 줌인 줌아웃 구현 (참고: https://error37.tistory.com/3)
    public float ZoomSpeed = 1.0f; //한번에 줌 되는 정도
    public float MinZoomSize = 3.0f; //최소 카메라 사이즈
    public float MaxZoomSize = 100.0f; //최대 카메라 사이즈

    private float targetZoomSize; //목표 카메라 크기

    //카메라 이동 구현 관련 변수 (참고: https://error37.tistory.com/2)
    public float DirectionForceReduceRate = 0.935f; //감속비율
    public float DirectionForceMin = 0.001f; //설정치 이하일 경우 움직임을 멈춤
    public float MaxX = 5f; //카메라 최대 X값
    public float MaxY = 5f; //카메라 최대 Y값
    public float MinX = -5f; //카메라 최소 X값
    public float MinY = -5f; //카메라 최소 Y값
    private bool _userMoveInput; //현재 마우스 클릭을 했냐 안했냐 판단
    private Vector3 _startPosition; //마우스 클릭 시작 위치를 기억
    private Vector3 _directionForce; //카메라 조작을 멈췄을때 서서히 감속하면서 이동
    private Camera _camera; //카메라 컴포넌트 받아오기


    private void Start()
    {
        _camera = GetComponent<Camera>();
        targetZoomSize = _camera.orthographicSize;
    }

    // Update is called once per frame
    private void Update()
    {
        ControllerZoom();

        UpdateZoom();

        ControlCameraPosition();

        ReduceDirectionForce();

        UpdateCameraPosition();
    }

    private void ControllerZoom()
    {

        var scrollInput = Input.GetAxis("Mouse ScrollWheel"); //마우스 스크롤 입력받기
        var hasScrollInput = Mathf.Abs(scrollInput) > Mathf.Epsilon;
        if (!hasScrollInput)
        {
            return;
        }
        var newSize = _camera.orthographicSize - scrollInput * ZoomSpeed; //기존 카메라 사이즈 - 스크롤 입력값 * ZoomSpeed
        targetZoomSize = Mathf.Clamp(newSize, MinZoomSize, MaxZoomSize); //카메라 크기 값을 최소값과 최대값 사이로 유지
    }

    private void UpdateZoom()
    {
        if (Mathf.Abs(targetZoomSize - _camera.orthographicSize) < Mathf.Epsilon)
        {
            return;
        }

        var mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
        var cameraTransform = gameObject.transform;
        var currentCameraPosition = cameraTransform.position;
        var offsetCamera = mouseWorldPos - currentCameraPosition - (mouseWorldPos - currentCameraPosition) / (_camera.orthographicSize / targetZoomSize);

        _camera.orthographicSize = targetZoomSize; // 카메라 크기 업데이트
        currentCameraPosition += offsetCamera; // 카메라 위치 조정
        cameraTransform.position = currentCameraPosition;

    }

    private void ControlCameraPosition()
    {
        var mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0)) //마우스 왼쪽을 클릭했을 때
        {
            CameraPositionMoveStart(mouseWorldPos);
        }
        else if (Input.GetMouseButton(0)) //마우스 클릭을 유지하고 있을 때
        {
            CameraPositionMoveProgress(mouseWorldPos);
        }
        else
        {
            CameraPositionMoveEnd();
        }
    }

    private void CameraPositionMoveStart(Vector3 startPosition)
    {
        _userMoveInput = true;
        _startPosition = startPosition;
        _directionForce = Vector2.zero;
    }

    private void CameraPositionMoveProgress(Vector3 targetPosition)
    {
        if (!_userMoveInput)
        {
            CameraPositionMoveStart(targetPosition);
            return;
        }

        _directionForce = _startPosition - targetPosition;
    }

    private void CameraPositionMoveEnd()
    {
        _userMoveInput = false;
    }

    private void ReduceDirectionForce()
    {
        if (_userMoveInput) //조작하고 있을 경우 바로 함수 종료
        {
            return;
        }

        _directionForce *= DirectionForceReduceRate; //감속 비율을 곱해서 감속 수치 적용

        // 설정치 이하일 경우 카메라를 강제로 멈춤
        if (_directionForce.magnitude < DirectionForceMin)
        {
            _directionForce = Vector3.zero;
        }
    }

    private void UpdateCameraPosition()
    {
        //이동 수치가 없으면 아무것도 안함
        if (_directionForce == Vector3.zero)
        {
            return;
        }

        var currentCameraPosition = transform.position;
        var targetCameraPosition = currentCameraPosition + _directionForce;
        // 카메라가 최대로 이동할 수 있는 위치 조정
        if (targetCameraPosition.x > MaxX)
        {
            targetCameraPosition = new Vector3(MaxX, targetCameraPosition.y, targetCameraPosition.z);
        }
        if (targetCameraPosition.x < MinX)
        {
            targetCameraPosition = new Vector3(MinX, targetCameraPosition.y, targetCameraPosition.z);
        }
        if (targetCameraPosition.y > MaxY)
        {
            targetCameraPosition = new Vector3(targetCameraPosition.x, MaxY, targetCameraPosition.z);
        }
        if (targetCameraPosition.y < MinY)
        {
            targetCameraPosition = new Vector3(targetCameraPosition.x, MinY, targetCameraPosition.z);
        }

        //선형보간 Lerp를 통한 부드러운 움직임 구현
        transform.position = Vector3.Lerp(currentCameraPosition, targetCameraPosition, 0.5f);
    }
}
