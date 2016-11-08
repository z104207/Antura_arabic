﻿using System;
using TMPro;
using UnityEngine;
using System.Collections;

namespace EA4S.SickLetters
{
    public class PlayGameState : IGameState
    {
        SickLettersGame game;

        
        

        Vector3 correctDotPos;


        float timer = 2, t = 0;
        public PlayGameState(SickLettersGame game)
        {
            this.game = game;
        }

        public void EnterState()
        {

            Debug.Log("enterplay");
            timer = game.gameDuration;

            SickLettersConfiguration.Instance.Context.GetAudioManager().MusicEnabled = true;
            SickLettersConfiguration.Instance.Context.GetAudioManager().PlayMusic(Music.MainTheme);

            game.LLPrefab.jumpIn();
        }

        public void ExitState()
        {
        }

        public void Update(float delta)
        {
            timer -= delta;

            if (timer < 0 /*|| game.successRoundsCount == 6*/)
            {
                game.SetCurrentState(game.ResultState);
                
            }

             if (Input.GetKeyDown(KeyCode.A))
             {
                 t = 1;
                 game.LLPrefab.jumpOut();
                 //game.LLPrefab.jumpIn();
            }

            correctDotPos = game.LLPrefab.correctDot.transform.TransformPoint(Vector3.Lerp(game.LLPrefab.correctDot.mesh.vertices[0], game.LLPrefab.correctDot.mesh.vertices[2], 0.5f));
            //scatterDDs();

            if(game.LLPrefab.correctDotCollider.transform.childCount == 0)
                game.LLPrefab.correctDotCollider.transform.position = correctDotPos;

            Debug.DrawRay(correctDotPos, -Vector3.forward * 10, Color.red);
            Debug.DrawRay(correctDotPos, -Vector3.right * 10, Color.yellow);
            //Debug.Log(v);
        }

        

        public void UpdatePhysics(float delta)
        {
            
        }

            
        
    }
}
