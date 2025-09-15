using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
//using System.Threading;

namespace Card_Game_Simulations
{
    public partial class Form_UltimateTexasHoldem : Form
    {
        bool simOn = false;
        bool paused = false;
        int simulations = 0;
        int currSimulations = 0;
        UltimateTexasHoldemData data = new UltimateTexasHoldemData();
        UltimateTexasHoldemData simData = new UltimateTexasHoldemData();
        //List<UltimateTexasHoldemPlayerData> playerData = new List<UltimateTexasHoldemPlayerData>();
        int maxPlayers = 6;
        int currPlayers = 0;
        Card[] board;
        List<Tuple<Card, Card>> playerCards;
        Tuple<Card, Card> dealerCards;
        Hand dealerHand;
        List<Hand> playerHands = new List<Hand>();
        Dictionary<Tuple<Card.CardSuit, int, Card.CardColor>, Image> cardImages = new Dictionary<Tuple<Card.CardSuit, int, Card.CardColor>, Image>();
        Card cardBack;
        Deck deck = new Deck();
        List<Timer> timers = new List<Timer>();
        Label[,] detailsTableLabels = new Label[3, 6];
        Label[,] handDetailsTableLabels = new Label[3, 10];
        bool[] winStoppers = new bool[3];
        bool[] handStoppers = new bool[10];
        const int stopHandDefault = -2;
        int stopHand = stopHandDefault;
        enum OptimalStrategy
        {
            None = 0,
            Yes,
            Suited,
            No
        }
        Dictionary<Label, Tuple<int, int>> optimalStrategyLabels = new Dictionary<Label, Tuple<int, int>>();
        Dictionary<Tuple<int, int>, OptimalStrategy> optimalStrategy = new Dictionary<Tuple<int, int>, OptimalStrategy>();

        public int CurrPlayer
        {
            get
            {
                return comboBox_PlayerHands.SelectedIndex + 1;
            }
            set
            {
                comboBox_PlayerHands.SelectedIndex = value;
                comboBox_PlayerHands.SelectedItem = comboBox_PlayerHands.Items[value];
            }
        }

        public Form_UltimateTexasHoldem()
        {
            InitializeComponent();
            FormClosed += Form_UltimateTexasHoldem_FormClosed;

            SetupTimers();
            SetupCards();

            SetupTableLayoutPanels();
            UpdateHandStoppers();

            SetupSimulations();
            SetupCurrPlayers();
            
            UpdateDataLabels();
        }

        void Form_UltimateTexasHoldem_Load(object sender, EventArgs e)
        {
            CenterToScreen();
        }
        void Form_UltimateTexasHoldem_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
        void button_Menu_Click(object sender, EventArgs e)
        {
            Form_Main form_Main = new Form_Main();

            form_Main.Show();

            Hide();
        }
        void button_StartPause_Click(object sender, EventArgs e)
        {
            if (simOn)
            {
                if (paused)
                {
                    ResumeSim();
                    ShowCards(false);
                    ShowCardDetails(false);
                }
                else
                    PauseSim();
            }
            else
            {
                StartSim();
                ShowCardDetails(false);
            }
        }
        void button_Next_Click(object sender, EventArgs e)
        {
            if (!simOn)
                StartSim(false);

            if (currSimulations < simulations)
                NextSim(sender, e);
        }
        void button_EndSim_Click(object sender, EventArgs e)
        {
            EndSim(false);
            ShowCardDetails(false);
        }
        void textBox_Simulations_KeyPress(object sender, KeyPressEventArgs e)
        {
            HandleTextBoxDigits(e);
        }
        void textBox_NumPlayers_KeyPress(object sender, KeyPressEventArgs e)
        {
            HandleTextBoxDigits(e);
        }
        void comboBox_PlayerHands_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (simOn && paused)
            {
                ShowPlayerCards(true);

                ShowCardDetails(true);
            }

            UnFocus();
        }
        void checkBox_WinStopperCheckChanged(object sender, EventArgs e)
        {
            UpdateWinStoppers();
        }
        void checkBox_HandStopperCheckChanged(object sender, EventArgs e)
        {
            UpdateHandStoppers();
        }
        void tableLayoutPanelLabel_OS_Click(object sender, EventArgs e)
        {
            CycleOptimalStrategyCell((Label)sender);
        }

        void SetupSimulations()
        {
            int.TryParse(textBox_Simulations.Text, out simulations);

            currSimulations = 0;
        }
        void SetupCurrPlayers()
        {
            int.TryParse(textBox_NumPlayers.Text, out currPlayers);
            if (currPlayers > maxPlayers)
            {
                currPlayers = maxPlayers;
                textBox_NumPlayers.Text = currPlayers.ToString();
            }
            else if (currPlayers < 1)
            {
                currPlayers = 1;
                textBox_NumPlayers.Text = currPlayers.ToString();
            }

            int index = comboBox_PlayerHands.SelectedIndex;

            for (int i = comboBox_PlayerHands.Items.Count - 1; i >= currPlayers; i--)
                comboBox_PlayerHands.Items.RemoveAt(i);

            for (int i = comboBox_PlayerHands.Items.Count; i < currPlayers; i++)
                comboBox_PlayerHands.Items.Add($"{i + 1}");

            if (index > 0 && index < currPlayers)
                CurrPlayer = index;
            else
                CurrPlayer = 0;
        }
        void SetupTimers()
        {
            timers.Add(new Timer());
            timers[0].Interval = 1;
            timers[0].Tick += RunSim;
        }
        void SetupCards()
        {
            deck.RemoveJokers(2);

            cardBack = new Card(Card.CardSuit.None, 0);
            Tuple<Card.CardSuit, int, Card.CardColor> t = new Tuple<Card.CardSuit, int, Card.CardColor>(cardBack.Suit, cardBack.Rank, cardBack.Card_Color);

            cardImages.Add(t, Image.FromFile(Path.GetFullPath($"Images/cardback.png")));

            for (int i = 0; i < 52; i++)
            {
                Card card = deck.Cards[i];

                string suit = card.Suit.ToString().ToLower();

                int r = card.Rank;
                string rank;
                if (r > 10)
                {
                    if (r == 11)
                        rank = "jack";
                    else if (r == 12)
                        rank = "queen";
                    else if (r == 13)
                        rank = "king";
                    else
                        rank = "ace";
                }
                else
                    rank = r.ToString();

                t = new Tuple<Card.CardSuit, int, Card.CardColor>(card.Suit, card.Rank, card.Card_Color);
                cardImages.Add(t, Image.FromFile(Path.GetFullPath($"Images/{suit}_{rank}.png")));
            }

            t = new Tuple<Card.CardSuit, int, Card.CardColor>(Card.CardSuit.Joker, 1, Card.CardColor.Black);
            cardImages.Add(t, Image.FromFile(Path.GetFullPath($"Images/joker_black.png")));

            t = new Tuple<Card.CardSuit, int, Card.CardColor>(Card.CardSuit.Joker, 1, Card.CardColor.Red);
            cardImages.Add(t, Image.FromFile(Path.GetFullPath($"Images/joker_red.png")));
        }
        void SetupTableLayoutPanels()
        {
            SetupDetailsTableLayoutControls();

            SetupHandDetailsTableControls();

            SetupOptimalStrategyTable();
        }
        void SetupDetailsTableLayoutControls()
        {
            for (int columns = 0; columns < 3; columns++)
            {
                for (int rows = 0; rows < 6; rows++)
                {
                    Label l = detailsTableLabels[columns, rows] = new Label();
                    l.TextAlign = ContentAlignment.MiddleLeft;

                    tableLayoutPanel_Details.Controls.Add(l, columns, rows);
                }
            }
        }
        void SetupHandDetailsTableControls()
        {
            for (int columns = 0; columns < 3; columns++)
            {
                for (int rows = 0; rows < 10; rows++)
                {
                    Label l = handDetailsTableLabels[columns, rows] = new Label();
                    l.TextAlign = ContentAlignment.MiddleLeft;

                    tableLayoutPanel_HandDetails.Controls.Add(l, columns, rows);
                }
            }
        }
        void SetupOptimalStrategyTable()
        {
            for (int columns = 0; columns < 13; columns++)
            {
                for (int rows = 0; rows < 13; rows++)
                {
                    Label l = new Label();

                    tableLayoutPanel_OS.Controls.Add(l, columns + 1, rows + 1);

                    l.Click += tableLayoutPanelLabel_OS_Click;
                    l.Size = new Size(35, 20);
                    l.TextAlign = ContentAlignment.MiddleLeft;
                }
            }

            // ace with any
            for (int i = 1; i < 14; i++)
                SetOptimalStrategyCell(i, 1, OptimalStrategy.Yes);

            // king with <5 suited
            for (int i = 1; i < 4; i++)
                SetOptimalStrategyCell(i, 2, OptimalStrategy.Suited);
            // king with >=5 unsuited
            for (int i = 4; i < 13; i++)
                SetOptimalStrategyCell(i, 2, OptimalStrategy.Yes);

            // queen with <6 no
            for (int i = 1; i < 5; i++)
                SetOptimalStrategyCell(i, 3, OptimalStrategy.No);
            // queen with >=6 suited
            for (int i = 5; i < 7; i++)
                SetOptimalStrategyCell(i, 3, OptimalStrategy.Suited);
            // queen with >=8 unsuited
            for (int i = 7; i < 12; i++)
                SetOptimalStrategyCell(i, 3, OptimalStrategy.Yes);

            // jack with <8 no
            for (int i = 1; i < 7; i++)
                SetOptimalStrategyCell(i, 4, OptimalStrategy.No);
            // jack with >=8 suited
            for (int i = 7; i < 9; i++)
                SetOptimalStrategyCell(i, 4, OptimalStrategy.Suited);
            // jack with >=10 unsuited
            for (int i = 9; i < 11; i++)
                SetOptimalStrategyCell(i, 4, OptimalStrategy.Yes);

            // 10 and below (except 2s), only pairs
            for (int i = 0; i < 8; i++)
            {
                for (int j = 1; j < 9 - i; j++)
                    SetOptimalStrategyCell(j, i + 5, OptimalStrategy.No);
                for (int j = 9 - i; j < 10 - i; j++)
                    SetOptimalStrategyCell(j, i + 5, OptimalStrategy.Yes);
            }
            SetOptimalStrategyCell(1, 13, OptimalStrategy.No);
        }
        void SetOptimalStrategyCell(int column, int row, OptimalStrategy os)
        {
            Label l = (Label)tableLayoutPanel_OS.GetControlFromPosition(column, row);

            l.TextAlign = ContentAlignment.MiddleCenter;

            SetOptimalStrategyLabel(l, os);

            Tuple<int, int> coords = new Tuple<int, int>(column, row);
            optimalStrategyLabels.Add(l, coords);
            optimalStrategy.Add(coords, os);
        }
        void CycleOptimalStrategyCell(Label l)
        {
            if (optimalStrategyLabels.ContainsKey(l))
            {
                Tuple<int, int> coords = optimalStrategyLabels[l];
                OptimalStrategy os = optimalStrategy[coords];

                OptimalStrategy newOS;
                if (os == OptimalStrategy.Yes)
                    newOS = OptimalStrategy.Suited;
                else if (os == OptimalStrategy.Suited)
                    newOS = OptimalStrategy.No;
                else
                    newOS = OptimalStrategy.Yes;

                SetOptimalStrategyLabel(l, newOS);

                optimalStrategy[coords] = newOS;
            }
        }
        void SetOptimalStrategyLabel(Label l, OptimalStrategy os)
        {
            if (os == OptimalStrategy.Yes)
            {
                l.BackColor = Color.FromName("MediumAquamarine");
                l.Text = "Y";
            }
            else if (os == OptimalStrategy.No)
            {
                l.BackColor = Color.FromName("LightCoral");
                l.Text = "N";
            }
            else
            {
                l.BackColor = Color.FromName("Gold");
                l.Text = "S";
            }
        }

        void ResetSim()
        {
            SetupSimulations();
            textBox_Simulations.Enabled = false;

            SetupCurrPlayers();
            textBox_NumPlayers.Enabled = false;
            
            data = new UltimateTexasHoldemData();

            //playerData.Clear();
            //for (int i = 0; i < currPlayers; i++)
            //{
            //    UltimateTexasHoldemPlayerData pD = new UltimateTexasHoldemPlayerData();
            //    pD.initialChips = 1000;
            //    pD.currentChips = pD.initialChips;

            //    playerData.Add(pD);
            //}
        }
        void StartSim(bool startTimer = true)
        {
            ResetSim();

            simOn = true;

            if (startTimer)
                timers[0].Start();

            button_StartPause.Text = "Pause";
        }
        void PauseSim()
        {
            paused = true;

            timers[0].Stop();

            button_StartPause.Text = "Resume";
        }
        void NextSim(object sender, EventArgs e)
        {
            PauseSim();

            RunSim(sender, e);

            ShowCards(true);

            ShowCardDetails(true);
        }
        void ResumeSim()
        {
            paused = false;

            timers[0].Start();

            button_StartPause.Text = "Pause";

            ShowCardDetails(false);
        }
        void RunSim(object sender, EventArgs e)
        {
            currSimulations++;

            deck.Shuffle();

            board = new Card[5];
            for (int i = 0; i < board.Length; i++)
                board[i] = deck.DealCard();

            Card[] flopCards = new Card[3]
            {
                board[0],
                board[1],
                board[2]
            };

            playerCards = new List<Tuple<Card, Card>>();
            for (int i = 0; i < currPlayers; i++)
                playerCards.Add(new Tuple<Card, Card>(deck.DealCard(), deck.DealCard()));

            simData = new UltimateTexasHoldemData();

            dealerCards = new Tuple<Card, Card>(deck.DealCard(), deck.DealCard());
            dealerHand = new Hand(board, dealerCards);

            UpdateData(dealerHand, data);
            UpdateData(dealerHand, simData);

            playerHands.Clear();
            for (int i = 0; i < currPlayers; i++)
            {
                //UltimateTexasHoldemPlayerData pD = playerData[i];
                //pD.hands++;
                //pD.handOutcomes++;
                //pD.currentBets = new UltimateTexasHoldemPlayerData.CurrentBets();

                //int anteBlind = 5;
                //int trips = 5;
                //int ultPair = 5;

                //pD.currentBets.chips_Ante = anteBlind;
                //pD.currentBets.chips_Blind = anteBlind;
                //pD.currentBets.chips_Trips = trips;
                //pD.currentBets.chips_UltPair = ultPair;

                Hand flopHand = new Hand(flopCards, playerCards[i]);
                Hand boardHand = new Hand(board);
                Hand fullHand = new Hand(board, playerCards[i]);

                //if (CheckBetPreFlop(playerCards[i], pD)) { }
                //else if (CheckBetFlop(playerCards[i], flopHand, pD)) { }
                //else if (CheckBetFull(playerCards[i], boardHand, fullHand, pD)) { }
                
                playerHands.Add(fullHand);

                Hand.WinStatus winStatus = Hand.HandComparison(dealerHand, playerHands[i]);
                switch (winStatus)
                {
                    case Hand.WinStatus.Win:
                        data.dealerWins++;
                        simData.dealerWins++;
                        //pD.losses++;
                        break;
                    case Hand.WinStatus.Lose:
                        data.playerWins++;
                        simData.playerWins++;
                        //pD.wins++;
                        break;
                    default:
                        data.pushes++;
                        simData.pushes++;
                        //pD.pushes++;
                        break;
                }

                CheckWinStop(winStatus, i);

                UpdateData(playerHands[i], data, i);
                data.handOutcomes++;

                UpdateData(playerHands[i], simData, i);
                simData.handOutcomes++;
            }

            deck.CollectCards();

            bool showCards = false;
            if (stopHand > stopHandDefault)
            {
                stopHand = stopHandDefault;
                showCards = true;

                if (!paused)
                    PauseSim();

                ShowCards(true);

                ShowCardDetails(true);
            }

            UpdateDataLabels();

            if (currSimulations == simulations)
                EndSim(showCards);
        }
        void EndSim(bool showCards)
        {
            simOn = false;
            paused = false;

            timers[0].Stop();

            button_StartPause.Text = "Start";

            textBox_Simulations.Enabled = true;
            textBox_NumPlayers.Enabled = true;

            ShowCards(showCards);
        }

        void UpdateDataLabels()
        {
            UpdateProgressBar();

            UpdateSimulationsLabel();

            UpdateDataLabel(1, data.hands, simData.hands, false);
            UpdateDataLabel(2, data.handOutcomes, simData.handOutcomes, false);

            UpdateDataLabel(3, data.dealerWins, simData.dealerWins);
            UpdateDataLabel(4, data.playerWins, simData.playerWins);
            UpdateDataLabel(5, data.pushes, simData.pushes);

            UpdateDataLabel(Hand.PokerHandRank.High_Card, data.highCards, simData.highCards);
            UpdateDataLabel(Hand.PokerHandRank.Pair, data.pairs, simData.pairs);
            UpdateDataLabel(Hand.PokerHandRank.Two_Pair, data.twoPairs, simData.twoPairs);
            UpdateDataLabel(Hand.PokerHandRank.Trips, data.trips, simData.trips);
            UpdateDataLabel(Hand.PokerHandRank.Straight, data.straights, simData.straights);
            UpdateDataLabel(Hand.PokerHandRank.Flush, data.flushes, simData.flushes);
            UpdateDataLabel(Hand.PokerHandRank.Full_House, data.fullHouses, simData.fullHouses);
            UpdateDataLabel(Hand.PokerHandRank.Quads, data.quads, simData.quads);
            UpdateDataLabel(Hand.PokerHandRank.Straight_Flush, data.straightFlushes, simData.straightFlushes);
            UpdateDataLabel(Hand.PokerHandRank.Royal_Flush, data.royalFlushes, simData.royalFlushes);
        }
        void UpdateProgressBar()
        {
            progressBar_Sim.Value = (int)((float)currSimulations / simulations * progressBar_Sim.Maximum);
        }
        void UpdateSimulationsLabel()
        {
            detailsTableLabels[0, 0].Text = currSimulations.ToString();
            if (currSimulations > 0)
            {
                float percentage = 100 * (float)currSimulations / simulations;
                if (percentage < 100)
                    detailsTableLabels[2, 0].Text = $"{percentage.ToString("n3")} %";
                else
                    detailsTableLabels[2, 0].Text = "100 %";
            }
            else
                detailsTableLabels[2, 0].Text = "";
        }
        void UpdateDataLabel(int index, int value, int currValue, bool showPercentage = true)
        {
            detailsTableLabels[0, index].Text = value.ToString();

            if (paused && currValue > 0)
                detailsTableLabels[1, index].Text = $"+{currValue}";
            else
                detailsTableLabels[1, index].Text = "";

            if (data.handOutcomes > 0 && showPercentage)
            {
                float percentage = 100 * (float)value / data.handOutcomes;
                detailsTableLabels[2, index].Text = $"{percentage.ToString("n3")} %";
            }
        }
        void UpdateDataLabel(Hand.PokerHandRank handRank, int value, int currValue)
        {
            int index = (int)handRank;

            handDetailsTableLabels[0, index].Text = value.ToString();

            if (paused && currValue > 0)
                handDetailsTableLabels[1, index].Text = $"+{currValue}";
            else
                handDetailsTableLabels[1, index].Text = "";

            if (data.hands > 0)
            {
                float percentage = 100 * (float)value / data.hands;
                handDetailsTableLabels[2, index].Text = $"{percentage.ToString("n3")} %";
            }
        }
        void UpdateData(Hand hand, UltimateTexasHoldemData _data, int playerNum = -1)
        {
            _data.hands++;

            switch (hand.handRank)
            {
                case Hand.PokerHandRank.High_Card:
                    _data.highCards++;
                    break;
                case Hand.PokerHandRank.Pair:
                    _data.pairs++;
                    break;
                case Hand.PokerHandRank.Two_Pair:
                    _data.twoPairs++;
                    break;
                case Hand.PokerHandRank.Trips:
                    _data.trips++;
                    break;
                case Hand.PokerHandRank.Straight:
                    _data.straights++;
                    break;
                case Hand.PokerHandRank.Flush:
                    _data.flushes++;
                    break;
                case Hand.PokerHandRank.Full_House:
                    _data.fullHouses++;
                    break;
                case Hand.PokerHandRank.Quads:
                    _data.quads++;
                    break;
                case Hand.PokerHandRank.Straight_Flush:
                    _data.straightFlushes++;
                    break;
                case Hand.PokerHandRank.Royal_Flush:
                    _data.royalFlushes++;
                    break;
                default:
                    break;
            }

            CheckHandStop(hand.handRank, playerNum);
        }
        void UpdateWinStoppers()
        {
            winStoppers[0] = checkBox_DealerWins.Checked;
            winStoppers[1] = checkBox_PlayerWins.Checked;
            winStoppers[2] = checkBox_Pushes.Checked;
        }
        void UpdateHandStoppers()
        {
            handStoppers[0] = checkBox_HighCards.Checked;

            handStoppers[1] = checkBox_Pairs.Checked;
            handStoppers[2] = checkBox_TwoPairs.Checked;

            handStoppers[3] = checkBox_Trips.Checked;
            handStoppers[4] = checkBox_Straights.Checked;
            handStoppers[5] = checkBox_Flushes.Checked;
            handStoppers[6] = checkBox_FullHouses.Checked;

            handStoppers[7] = checkBox_Quads.Checked;
            handStoppers[8] = checkBox_StraightFlushes.Checked;
            handStoppers[9] = checkBox_RoyalFlushes.Checked;
        }
        string UpdateHandText(Hand hand)
        {
            string handText = $"{hand.handRank}\n\n";
            for (int i = 0; i < hand.cards.Length; i++)
                handText += $"{hand.cards[i].RankSymbol}  ";
            return handText;
        }

        void CheckWinStop(Hand.WinStatus winStatus, int playerNum)
        {
            int index = (int)winStatus;
            if (winStoppers[index])
            {
                if (stopHand == stopHandDefault)
                {
                    stopHand = index;
                    if (playerNum > -1)
                        CurrPlayer = playerNum;
                    else if (comboBox_PlayerHands.SelectedIndex == -1)
                        CurrPlayer = 0;
                }
            }
        }
        void CheckHandStop(Hand.PokerHandRank handRank, int playerNum)
        {
            int index = (int)handRank;
            if (handStoppers[index])
            {
                if (stopHand == stopHandDefault || index > stopHand)
                {
                    stopHand = index;
                    if (playerNum > -1)
                        CurrPlayer = playerNum;
                    else if (comboBox_PlayerHands.SelectedIndex == -1)
                        CurrPlayer = 0;
                }
            }
        }
        void ShowCardDetails(bool show)
        {
            string winnerT;
            string dealerT;
            string playerT;
            if (show)
            {
                int playerNum = comboBox_PlayerHands.SelectedIndex;
                Hand.WinStatus winStatus = Hand.HandComparison(dealerHand, playerHands[playerNum]);
                if (winStatus == Hand.WinStatus.Win)
                    winnerT = "Dealer Wins";
                else if (winStatus == Hand.WinStatus.Lose)
                    winnerT = $"Player {CurrPlayer} Wins";
                else
                {
                    if (ChopChop())
                        winnerT = "Chop Chop";
                    else
                        winnerT = "Push";
                }
                
                dealerT = UpdateHandText(dealerHand);

                playerT = UpdateHandText(playerHands[playerNum]);
            }
            else
            {
                winnerT = "The House Always Wins!";

                dealerT = playerT = "-";
            }

            label_Winner.Text = winnerT;

            label_DealerHand.Text = dealerT;

            label_PlayerHand.Text = playerT;
        }
        void ShowCards(bool show)
        {
            ShowBoardCards(show);

            ShowDealerCards(show);

            ShowPlayerCards(show);
        }
        void ShowBoardCards(bool show)
        {
            if (show)
            {
                ShowCard(pictureBox_BoardCard1, board[0]);
                ShowCard(pictureBox_BoardCard2, board[1]);
                ShowCard(pictureBox_BoardCard3, board[2]);
                ShowCard(pictureBox_BoardCard4, board[3]);
                ShowCard(pictureBox_BoardCard5, board[4]);
            }
            else
            {
                ShowCard(pictureBox_BoardCard1);
                ShowCard(pictureBox_BoardCard2);
                ShowCard(pictureBox_BoardCard3);
                ShowCard(pictureBox_BoardCard4);
                ShowCard(pictureBox_BoardCard5);
            }
        }
        void ShowDealerCards(bool show)
        {
            if (show)
            {
                ShowCard(pictureBox_DealerCard1, dealerCards.Item1);
                ShowCard(pictureBox_DealerCard2, dealerCards.Item2);
            }
            else
            {
                ShowCard(pictureBox_DealerCard1);
                ShowCard(pictureBox_DealerCard2);
            }
        }
        void ShowPlayerCards(bool show)
        {
            if (show)
            {
                int currPlayer = comboBox_PlayerHands.SelectedIndex;

                ShowCard(pictureBox_PlayerCard1, playerCards[currPlayer].Item1);
                ShowCard(pictureBox_PlayerCard2, playerCards[currPlayer].Item2);
            }
            else
            {
                ShowCard(pictureBox_PlayerCard1);
                ShowCard(pictureBox_PlayerCard2);
            }
        }
        void ShowCard(PictureBox picBox, Card card = null)
        {
            if (card == null)
                card = cardBack;

            Tuple<Card.CardSuit, int, Card.CardColor> t = new Tuple<Card.CardSuit, int, Card.CardColor>(card.Suit, card.Rank, card.Card_Color);

            picBox.Image = cardImages[t];
        }
        //bool CheckBetPreFlop(Tuple<Card, Card> cards, UltimateTexasHoldemPlayerData pD)
        //{
        //    int column = cards.Item1.Rank;
        //    int row = cards.Item2.Rank;

        //    Tuple<int, int> coords = null;
        //    Tuple<int, int> coords1 = new Tuple<int, int>(column - 1, 15 - row);
        //    Tuple<int, int> coords2 = new Tuple<int, int>(row - 1, 15 - column);

        //    bool suited = cards.Item1.Suit == cards.Item2.Suit;

        //    if (optimalStrategy.ContainsKey(coords1))
        //        coords = coords1;
        //    else if (optimalStrategy.ContainsKey(coords2))
        //        coords = coords2;

        //    if (coords != null)
        //    {
        //        if (optimalStrategy[coords] == OptimalStrategy.Yes || (suited && optimalStrategy[coords] == OptimalStrategy.Suited))
        //        {
        //            pD.betPreFlop++;
        //            pD.currentBets.chips_Play += pD.currentBets.chips_Ante * 4;
        //            return true;
        //        }
        //    }

        //    return false;
        //}
        //bool CheckBetFlop(Tuple<Card, Card> cards, Hand flopHand, UltimateTexasHoldemPlayerData pD)
        //{
        //    bool bet = false;
        //    // has two pair or better
        //    if (flopHand.handRank >= Hand.PokerHandRank.Two_Pair)
        //        bet = true;
        //    // has hidden pair
        //    else if (flopHand.handRank == Hand.PokerHandRank.Pair && flopHand.cards[0].Rank > 2)
        //    {
        //        if (cards.Item1.Rank == flopHand.cards[0].Rank || cards.Item2.Rank == flopHand.cards[0].Rank)
        //            bet = true;
        //    }

        //    // has 4 to a flush with at least 10 high
        //    if (!bet)
        //    {
        //        List<List<Card>> suits = new List<List<Card>>();
        //        for (int i = 0; i < 4; i++)
        //            suits.Add(new List<Card>());

        //        for (int i = 0; i < flopHand.cards.Length; i++)
        //        {
        //            suits[(int)flopHand.cards[i].Suit - 1].Add(flopHand.cards[i]);
        //        }

        //        for (int i = 0; i < suits.Count; i++)
        //        {
        //            if (suits[i].Count == 4)
        //            {
        //                for (int j = 0; j < 4; j++)
        //                {
        //                    if (suits[i][j].Rank >= 10)
        //                    {
        //                        bet = true;
        //                        break;
        //                    }
        //                }
        //                break;
        //            }
        //        }
        //    }
            
        //    if (bet)
        //    {
        //        pD.betFlop++;
        //        pD.currentBets.chips_Play += pD.currentBets.chips_Ante * 2;
        //        return true;
        //    }
        //    else
        //        return false;
        //}
        //bool CheckBetFull(Tuple<Card, Card> cards, Hand boardHand, Hand fullHand, UltimateTexasHoldemPlayerData pD)
        //{
        //    bool bet = false;
        //    Hand.WinStatus winStatus = Hand.HandComparison(fullHand, boardHand);

        //    // if player beats the board
        //    if (winStatus == Hand.WinStatus.Win)
        //    {
        //        if (fullHand.handRank > boardHand.handRank)
        //            bet = true;
        //        if (fullHand.handRank >= Hand.PokerHandRank.Pair)
        //        {

        //        }
                
        //    }
            
        //    // count outs
        //    else
        //    {
        //        int outs = 0;
        //        List<int> outNums = new List<int>();

        //        // add combo outs
        //        for (int i = 0; i < boardHand.combos.Count; i++)
        //        {
        //            int tempOuts = 4 - boardHand.combos[i].Count;

        //            outs += tempOuts;
        //            outNums.Add(boardHand.combos[i][0].Rank);
        //        }

        //        // add kicker outs
        //        for (int i = 0; i < boardHand.cards.Length; i++)
        //        {
        //            bool add = true;
        //            for (int j = 0; j < boardHand.combos.Count; j++)
        //            {
        //                if (boardHand.cards[i].Rank == boardHand.combos[j][0].Rank)
        //                {
        //                    add = false;
        //                    continue;
        //                }
        //            }

        //            if (add)
        //            {
        //                outs += 3;
        //                outNums.Add(boardHand.cards[i].Rank);
        //            }
        //        }

        //        // add potential kicker outs
        //        int startRank = cards.Item1.Rank;
        //        if (cards.Item1.Rank < cards.Item2.Rank)
        //            startRank = cards.Item2.Rank;

        //        for (int i = startRank + 1; i < 15; i++)
        //        {
        //            bool add = true;
        //            for (int j = 0; j < boardHand.cards.Length; j++)
        //            {
        //                if (boardHand.cards[j].Rank == i)
        //                {
        //                    add = false;
        //                    break;
        //                }
        //            }

        //            if (add)
        //            {
        //                outs += 4;
        //                outNums.Add(boardHand.cards[i].Rank);
        //            }
        //        }

        //        // add flush outs
        //        //List<List<Card>> suits = new List<List<Card>>();
        //        //for (int i = 0; i < 4; i++)
        //        //    suits.Add(new List<Card>());

        //        //for (int i = 0; i < boardHand.cards.Length; i++)
        //        //{
        //        //    suits[(int)boardHand.cards[i].Suit - 1].Add(boardHand.cards[i]);
        //        //}

        //        //for (int i = 0; i < suits.Count; i++)
        //        //{
        //        //    if (suits[i].Count == 4)
        //        //    {
        //        //        for (int j = 0; j < 4; j++)
        //        //        {
        //        //            if (suits[i][j].Rank >= 10)
        //        //            {
        //        //                bet = true;
        //        //                break;
        //        //            }
        //        //        }
        //        //        break;
        //        //    }
        //        //}


        //        if (outs < 21)
        //            bet = true;

        //    }

        //    if (bet)
        //    {
        //        pD.betRiver++;
        //        pD.currentBets.chips_Play += pD.currentBets.chips_Ante;
        //        return true;
        //    }


        //    return true;
        //}
        void HandleTextBoxDigits(KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                if (e.KeyChar == '\r')
                    UnFocus();
                e.Handled = true;
            }
        }
        void UnFocus()
        {
            panel_CardDetails.Focus();
        }

        // meme
        bool ChopChop()
        {
            Random r = new Random();

            return r.Next(0, 1000) == 1;
        }
    }
}
