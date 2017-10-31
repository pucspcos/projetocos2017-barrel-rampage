using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FMOD;
using FMOD.Studio;
using FMODUnity;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public int m_NumRoundsToWin = 2;            // The number of rounds a single player has to win to win the game.
        public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases.
        public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
        public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
        public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
        public GameObject m_TankPrefab;             // Reference to the prefab the players will control.
        public TankManager[] m_Tanks;               // A collection of managers for enabling and disabling different aspects of the tanks.
        public int m_BarrelNumber, m_ActiveBarrels;
        public GameObject barrelPrefab;
        public static GameManager managerInstance;
        public Text timeText, pointsP1, pointsP2;
        public int barrelPointP1 = 0, barrelPointP2 = 0;
        public float timeLeft;
        public Transform[] barrelSpawns;
        public GameObject[] roundBarrels;
        public bool isRoundPlaying;
        public float soundTimer;
        public string soundtrack = "event:/Soundtrack";
        public EventInstance musicEvent;
        public ParameterInstance sampleInstance;

        private int m_RoundNumber;                  // Which round the game is currently on.
        private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts.
        private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends.
        private TankManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won.
        private TankManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won.

        void OnEnable()
        {
            if (managerInstance == null)
            {
                managerInstance = this;
            }

            else
            {
                UnityEngine.Debug.LogError("More than one Game Manager on the scene!");
            }
        }

        private void Start()
        {
            // Create the delays so they only have to be made once.
            m_StartWait = new WaitForSeconds(m_StartDelay);
            m_EndWait = new WaitForSeconds(m_EndDelay);
            roundBarrels = new GameObject[m_BarrelNumber];
            musicEvent = RuntimeManager.CreateInstance(soundtrack);
            musicEvent.getParameter("SampleInstance", out sampleInstance);
            musicEvent.start();
            isRoundPlaying = false;
            timeText.enabled = false;
            pointsP1.enabled = false;
            pointsP2.enabled = false;
            SpawnAllTanks();
            SetCameraTargets();
            //InstantiateBarrels();
            //SetBarrelsPosition();
            Invoke("UIActivation", 3);

            // Once the tanks have been created and the camera is using them as targets, start the game.
            StartCoroutine(GameLoop());
        }


        private void SpawnAllTanks()
        {
            // For all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... create them, set their player number and references needed for control.
                m_Tanks[i].m_Instance =
                    Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
                m_Tanks[i].m_PlayerNumber = i + 1;
                m_Tanks[i].Setup();
            }
        }

        void InstantiateBarrels()
        {
            for (int i = 0; i < m_BarrelNumber; i++)
            {
                roundBarrels[i] = Instantiate(barrelPrefab, barrelSpawns[i].position, barrelSpawns[i].rotation);
                //roundBarrels[i] = barrelPrefab;
                //barrelsProperties[i] = GetComponent<Explosive_Barrel>();
            }
        }

        private void SetCameraTargets()
        {
            // Create a collection of transforms the same size as the number of tanks.
            Transform[] targets = new Transform[m_Tanks.Length];

            // For each of these transforms...
            for (int i = 0; i < targets.Length; i++)
            {
                // ... set it to the appropriate tank transform.
                targets[i] = m_Tanks[i].m_Instance.transform;
            }

            // These are the targets the camera should follow.
            m_CameraControl.m_Targets = targets;
        }


        // This is called from start and will run each phase of the game one after another.
        private IEnumerator GameLoop()
        {
            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundStarting());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
            yield return StartCoroutine(RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found.
            if (m_GameWinner != null)
            {
                // If there is a game winner, restart the level.
                musicEvent.stop(STOP_MODE.IMMEDIATE);
                SceneManager.LoadScene("GameOver");
            }
            else
            {
                // If there isn't a winner yet, restart this coroutine so the loop continues.
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
                StartCoroutine(GameLoop());
            }
        }

        private void Update()
        {
            soundTimer += Time.deltaTime;
            if (soundTimer >= 7)
            {
                sampleInstance.setValue(UnityEngine.Random.Range(0, 3));
                soundTimer = 0;
            }
            OneTankLeft();
            if (timeText.IsActive() == true && isRoundPlaying)
                timeLeft -= Time.deltaTime;
            timeText.text = ("Time\n" + (int)timeLeft);

            if (timeLeft <= 1f)
            {
                m_ActiveBarrels = 0;
            }
        }


        private IEnumerator RoundStarting()
        {
            // As soon as the round starts reset the tanks and make sure they can't move.
            ResetAllTanks();
            DisableTankControl();
            InstantiateBarrels();
            //SetBarrelsPosition();
            Invoke("UIActivation", 3);

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            m_CameraControl.SetStartPositionAndSize();

            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_StartWait;
        }


        private IEnumerator RoundPlaying()
        {
            // As soon as the round begins playing let the players control the tanks.
            EnableTankControl();

            // Clear the text from the screen.
            m_MessageText.text = string.Empty;

            // While there is not one tank left...
            while (m_ActiveBarrels > 0)
            {
                // ... return on the next frame.
                yield return null;
            }
        }


        private IEnumerator RoundEnding()
        {
            // Stop tanks from moving.
            DisableTankControl();
            UIDeactivation();
            timeText.enabled = false;
            pointsP1.enabled = false;
            pointsP2.enabled = false;
            timeLeft = 90;

            // Clear the winner from the previous round.
            m_RoundWinner = null;
            m_ActiveBarrels = 10;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score.
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            // Now the winner's score has been incremented, see if someone has one the game.
            m_GameWinner = GetGameWinner();

            // Get a message based on the scores and whether or not there is a game winner and display it.
            string message = EndMessage();
            m_MessageText.text = message;
            barrelPointP1 = 0;
            barrelPointP2 = 0;
            pointsP1.text = ("Points P1: " + barrelPointP1);
            pointsP2.text = ("Points P2: " + barrelPointP2);
            DisableBarrels();
            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_EndWait;
        }

        // This is used to check if there is one or fewer tanks remaining and thus the round should end.
        private void OneTankLeft()
        {
            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if they are active, increment the counter.
                if (!m_Tanks[i].m_Instance.activeSelf)
                    ResetTankManager(i);
            }
        }

        void ResetTankManager(int tankNumber)
        {
            if (tankNumber == 0)
            {
                Invoke("ResetTankOne", 3);
            }

            if (tankNumber == 1)
            {
                Invoke("ResetTankTwo", 3);
            }
        }

        void ResetTankOne()
        {
            m_Tanks[0].Reset();
            SetCameraTargets();
        }

        void ResetTankTwo()
        {
            m_Tanks[1].Reset();
            SetCameraTargets();
        }


        // This function is to find out if there is a winner of the round.
        // This function is called with the assumption that 1 or fewer tanks are currently active.
        private TankManager GetRoundWinner()
        {
            if (managerInstance.barrelPointP1 > managerInstance.barrelPointP2)
            {
                return m_Tanks[0];
            }

            else if (managerInstance.barrelPointP1 < managerInstance.barrelPointP2)
            {
                return m_Tanks[1];
            }

            return null;

            /*
            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if one of them is active, it is the winner so return it.
                if (m_Tanks[i].m_Instance.activeSelf)
                    return m_Tanks[i];
            }*/

            // If none of the tanks are active it is a draw so return null.

        }


        // This function is to find out if there is a winner of the game.
        private TankManager GetGameWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if one of them has enough rounds to win the game, return it.
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                    return m_Tanks[i];
            }

            // If no tanks have enough rounds to win, return null.
            return null;
        }


        // Returns a string message to display at the end of each round.
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that.
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            // Add some line breaks after the initial message.
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message.
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
            }

            // If there is a game winner, change the entire message to reflect that.
            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties.
        private void ResetAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].Reset();
            }
        }


        private void EnableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].EnableControl();
            }
        }


        private void DisableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].DisableControl();
            }
        }

        public void playerOnePointUp()
        {
            barrelPointP1++;
            pointsP1.text = ("Points P1: " + barrelPointP1);
        }

        public void playerTwoPointUp()
        {
            barrelPointP2++;
            pointsP2.text = ("Points P2: " + barrelPointP2);
        }

        public void UIActivation()
        {
            timeText.enabled = true;
            pointsP1.enabled = true;
            pointsP2.enabled = true;
            isRoundPlaying = true;
        }

        void UIDeactivation()
        {
            timeText.enabled = false;
            pointsP1.enabled = false;
            pointsP2.enabled = false;
            isRoundPlaying = false;
        }

        public void DisableBarrels()
        {
            for (int i = 0; i < roundBarrels.Length; i++)
            {
                DestroyImmediate(roundBarrels[i], true);

            }

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("BarrelExplosionParticle"))
            {
                DestroyImmediate(go);
            }
        }
    }
}