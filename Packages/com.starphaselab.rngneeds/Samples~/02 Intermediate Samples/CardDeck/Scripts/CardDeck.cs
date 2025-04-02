using System.Collections.Generic;
using UnityEngine;

namespace RNGNeeds.Samples.CardDeck
{
    [CreateAssetMenu(fileName = "New Card Deck", menuName = "RNGNeeds/Card Decks/Card Deck")]
    public class CardDeck : ScriptableObject
    {
        public ProbabilityList<Card> cards;

        public Card Peek()
        {
            return cards.GetProbabilityItem(0).Value;
        }

        public List<Card> DrawFromTop(int count)
        {
            var pickedCards = new List<Card>();
            var drawCount = Mathf.Clamp(count, count, cards.GetTotalUnits());
         
            var cardIndex = 0;
            
            while(pickedCards.Count < drawCount && cardIndex <= cards.ItemCount)
            {
                if (cards.TryGetProbabilityItem(cardIndex, out var cardItem) == false) break;
                if (cardItem.IsDepletable == false || cardItem.IsSelectable == false)
                {
                    cardIndex++;
                    continue;
                }
                pickedCards.Add(cardItem.Value);
                cardItem.Units--;
            }

            return pickedCards;
        }
        
        public List<Card> DrawFromBottom(int count)
        {
            var pickedCards = new List<Card>();
            var drawCount = Mathf.Clamp(count, count, cards.GetTotalUnits());
         
            var cardIndex = cards.ItemCount - 1;
            
            while(pickedCards.Count < drawCount && cardIndex >= 0)
            {
                if (cards.TryGetProbabilityItem(cardIndex, out var cardItem) == false) break;
                if (cardItem.IsDepletable == false || cardItem.IsSelectable == false)
                {
                    cardIndex--;
                    continue;
                }
                pickedCards.Add(cardItem.Value);
                cardItem.Units--;
            }

            return pickedCards;
        }
        
        public List<Card> DrawRandom(int count)
        {
            var pickedCards = new List<Card>();
            pickedCards.AddRange(cards.PickValues(count));
            return pickedCards;
        }
    }
        
}