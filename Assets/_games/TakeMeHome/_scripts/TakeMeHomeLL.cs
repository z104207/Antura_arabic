﻿using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
using System.Collections;
using ArabicSupport;
namespace EA4S.TakeMeHome
{
public class TakeMeHomeLL : MonoBehaviour {

		public GameObject plane;
		public bool isDragged;
		public bool isMoving;
		public bool isDraggable;

		public Transform livingLetterTransform;
		public BoxCollider boxCollider;

		public LetterObjectView letter;

		Tweener moveTweener;
		Tweener rotationTweener;

		Vector3 holdPosition;
		Vector3 normalPosition;

		private float cameraDistance;

		float maxY;

		bool dropLetter;
		bool clampPosition;
		public bool dragging = false;
		Vector3 dragOffset = Vector3.zero;
		Vector3 tubeSpawnPosition;
		public bool respawn;

		public event Action onMouseUpLetter;

		Action endTransformToCallback;

		public TakeMeHomeTube lastTube;

		Transform[] letterPositions;
		int currentPosition;

		void Awake()
		{
			normalPosition = transform.localPosition;
			livingLetterTransform = transform;
			holdPosition.x = normalPosition.x;
			isMoving = false;
			isDraggable = false;
			holdPosition.y = normalPosition.y;
			lastTube = null;
			respawn = true;
		}

		public void Initialize(float _maxY, LetterObjectView _letter, Vector3 tubePosition)
		{
			tubeSpawnPosition = tubePosition;

			cameraDistance =  (transform.position.z) - Camera.main.transform.position.z;

			//cameraDistance = Vector3.Distance(Camera.main.transform.position, transform.position);
			letter = _letter;
			maxY = _maxY;

			dropLetter = false;

			clampPosition = false;

		}

		public void PlayIdleAnimation()
        {
            letter.SetState(LLAnimationStates.LL_idle);

			//livingLetterTransform.localPosition = normalPosition;
		}

		public void PlayWalkAnimation()
		{
			letter.SetState(LLAnimationStates.LL_walking);
            letter.SetWalkingSpeed(LetterObjectView.WALKING_SPEED);

			//livingLetterTransform.localPosition = normalPosition;
		}

		public void PlayHoldAnimation()
        {
            letter.SetState(LLAnimationStates.LL_idle);

            //livingLetterTransform.localPosition = holdPosition;
        }



		public void MoveTo(Vector3 position, float duration)
		{
			isMoving = true;
			PlayWalkAnimation();

			transform.rotation = Quaternion.Euler (new Vector3 (0, -90, 0));

			if (moveTweener != null)
			{
				moveTweener.Kill();
			}

			moveTweener = transform.DOLocalMove(position, duration).OnComplete(delegate () { 
				PlayIdleAnimation(); 
				if (endTransformToCallback != null) endTransformToCallback();

				//play audio
				TakeMeHomeConfiguration.Instance.Context.GetAudioManager().PlayLetterData(letter.Data, true);
				RotateTo(new Vector3 (0, 180, 0),0.5f);
				isMoving = false;
			});
		}

		public void MoveBy(Vector3 position, float duration)
		{
			MoveTo (transform.position + position, duration);
		}


		void RotateTo(Vector3 rotation, float duration)
		{
			if (rotationTweener != null)
			{
				rotationTweener.Kill();
			}

			rotationTweener = transform.DORotate(rotation, duration);
		}

		void TransformTo(Transform transformTo, float duration, Action callback)
		{
			MoveTo(transformTo.localPosition, duration);
			RotateTo(transformTo.eulerAngles, duration);

			endTransformToCallback = callback;
		}

		public void GoToFirstPostion()
		{
			GoToPosition(0);
		}

		public void GoToPosition(int positionNumber)
		{
			dropLetter = false;

			if (moveTweener != null) { moveTweener.Kill(); }
			if (rotationTweener != null) { rotationTweener.Kill(); }

			currentPosition = positionNumber;

			transform.localPosition = letterPositions[currentPosition].localPosition;
			transform.rotation = letterPositions[currentPosition].rotation;
		}

		public void MoveToNextPosition(float duration, Action callback)
		{
			dropLetter = false;

			if (moveTweener != null) { moveTweener.Kill(); }
			if (rotationTweener != null) { rotationTweener.Kill(); }

			currentPosition++;

			if (currentPosition >= letterPositions.Length)
			{
				currentPosition = 0;
			}

			TransformTo(letterPositions[currentPosition], duration, callback);
		}

		public void OnPointerDown(Vector2 pointerPosition)
		{
			if (isMoving || !isDraggable)
				return;
			
			if (!dragging)
			{
				lastTube = null;
				dragging = true;

				var data = letter.Data;

				TakeMeHomeConfiguration.Instance.Context.GetAudioManager().PlayLetterData(data, true);

				Vector3 mousePosition = new Vector3(pointerPosition.x, pointerPosition.y, cameraDistance);
				Vector3 world = Camera.main.ScreenToWorldPoint(mousePosition);
				dragOffset = world - transform.position;

				OnPointerDrag(pointerPosition);

				PlayHoldAnimation();
			}
		}

		public void OnPointerDrag(Vector2 pointerPosition)
		{
			if (dragging)
			{
				dropLetter = false;

				Vector3 mousePosition = new Vector3(pointerPosition.x, pointerPosition.y, cameraDistance);

				transform.position = Camera.main.ScreenToWorldPoint(mousePosition);

				transform.position = ClampPositionToStage(transform.position - dragOffset);
			}
		}

		public void OnPointerUp()
		{
			if (dragging)
			{
				dragging = false;
				dropLetter = true;

				//check if position should clamp:
				if (transform.position.x > 3 && transform.position.y > maxY)
					clampPosition = true;

				PlayIdleAnimation();

				if (onMouseUpLetter != null)
				{
					onMouseUpLetter();
				}


			}
		}

		void Drop(float delta)
		{
			Vector3 dropPosition = transform.position;

			dropPosition += Physics.gravity * delta;


			if(clampPosition) transform.position = ClampPositionToStage(dropPosition);
			else transform.position = dropPosition;//ClampPositionToStage(dropPosition);

			//free fall:
			if (!clampPosition) {
				if (respawn && transform.position.y < (maxY - 20)) {
					AudioManager.I.PlaySfx (Sfx.Splat);
					//transform.position = 
					transform.position = tubeSpawnPosition;
					clampPosition = true;
				}
			}
		}

		void Update()
		{
			if (dropLetter)
			{
				Drop(Time.deltaTime);
			}
		}

		Vector3 ClampPositionToStage(Vector3 unclampedPosition)
		{
			Vector3 clampedPosition = unclampedPosition;

			

			if(!dragging)
				clampedPosition.y = clampedPosition.y < maxY ? maxY : clampedPosition.y;

			if (clampedPosition.y == maxY) {
				dropLetter = false;
				clampPosition = false;

			}
			
			return clampedPosition;
		}

		private void moveUp()
		{
			if (lastTube == null)
				return;
			
			if (moveTweener != null)
			{
				moveTweener.Kill();
			}

			moveTweener = transform.DOLocalMove(transform.position + lastTube.transform.up*30, 1).OnComplete(delegate () { 
				PlayIdleAnimation(); 
				if (endTransformToCallback != null) endTransformToCallback();

				transform.rotation = Quaternion.Euler (new Vector3 (0, 180, 0));
				transform.position = tubeSpawnPosition;
				clampPosition = true;
				dropLetter = true;
				isMoving = false;
			});
		}

		public void panicAndRun()
		{
			isMoving = true;
			isDraggable = false;
			dropLetter = false;

			RotateTo(new Vector3 (0, -90, 0),0.5f);


			letter.SetState(LLAnimationStates.LL_walking);
			letter.SetWalkingSpeed(LetterObjectView.RUN_SPEED);

			if (moveTweener != null)
			{
				moveTweener.Kill();
			}

			moveTweener = transform.DOLocalMove(new Vector3(2,-6.52f,-15), 1).OnComplete(delegate () { 
				PlayIdleAnimation();
				respawn = false;
				clampPosition = false;
				dropLetter = true;
				isMoving = false;
			});

		}


		public void followTube(bool win)
		{
			



			isMoving = true;
			isDraggable = false;
			dropLetter = false;

			if (win) {
				letter.Poof ();
				letter.DoHighFive ();
				StartCoroutine (waitForSeconds (2));

				return;
			}




		
			moveUp ();
		


		}

		IEnumerator waitForSeconds(float seconds)
		{
			yield return new WaitForSeconds (seconds);
			moveUp ();

		}

		IEnumerator waitForSecondsAndJump(float seconds)
		{
			yield return new WaitForSeconds (seconds);
			letter.SetState(LLAnimationStates.LL_walking);
			letter.SetWalkingSpeed(LetterObjectView.RUN_SPEED);

			if (moveTweener != null)
			{
				moveTweener.Kill();
			}

			moveTweener = transform.DOLocalMove(transform.position - (new Vector3(5,0,0)), 1).OnComplete(delegate () { 
				
				clampPosition = false;
				dropLetter = true;
				isMoving = false;
			});
		}


		public void EnableCollider(bool enable)
		{
			boxCollider.enabled = enable;
		}

		void OnTriggerEnter(Collider other)
		{
			if (!dragging) {
				lastTube = null;
				return;
			}
			
			TakeMeHomeTube tube = other.gameObject.GetComponent<TakeMeHomeTube> ();
			if (!tube)
				return;

			lastTube = tube;
			tube.shake ();
		}

		void OnTriggerExit(Collider other)
		{
			//if (!dragging)
			//	return;


			TakeMeHomeTube tube = other.gameObject.GetComponent<TakeMeHomeTube> ();
			if (!tube)
				return;

			lastTube = null;
		}
	}

}
