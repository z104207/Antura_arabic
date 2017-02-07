﻿using EA4S.Antura;
using EA4S.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EA4S.Minigames.HideAndSeek
{
    public class AnturaPathFollower : MonoBehaviour
    {
        private List<Vector3> path;

        int currentNode = 0;
        float speed = 10f;

        float randomSniffTime;
        bool isSniffing = false;

        public bool IsFollowing { get { return path != null; } }

        AnturaAnimationController animationController;

        void Awake()
        {
            animationController = GetComponent<AnturaAnimationController>();
        }

        public void FollowPath(AnturaPath path)
        {
            this.path = path.GetPath();
            currentNode = 0;
            transform.position = this.path[0];
            animationController.State = AnturaAnimationStates.walking;
            randomSniffTime = UnityEngine.Random.Range(3, 6);
            isSniffing = false;
        }

        void Update()
        {
            if (path != null)
            {
                var target = path[currentNode];

                var distance = target - transform.position;
                distance.y = 0;

                if (distance.sqrMagnitude < 0.1f)
                {
                    // reached
                    ++currentNode;
                    if (currentNode >= path.Count)
                    {
                        animationController.State = AnturaAnimationStates.idle;
                        path = null;
                    }
                }
                else
                {
                    if (isSniffing)
                        return;

                    randomSniffTime -= Time.deltaTime;

                    if (randomSniffTime < 0)
                    {
                        randomSniffTime = UnityEngine.Random.Range(3, 6);
                        isSniffing = true;
                        animationController.DoSniff(() => { isSniffing = false; });
                    }
                    else
                    {
                        distance.Normalize();
                        transform.position += distance * Mathf.Abs(Vector3.Dot(distance, transform.forward)) * speed * Time.deltaTime;
                        MathUtils.LerpLookAtPlanar(transform, target, Time.deltaTime * 2);
                    }
                }
            }
        }
    }
}
