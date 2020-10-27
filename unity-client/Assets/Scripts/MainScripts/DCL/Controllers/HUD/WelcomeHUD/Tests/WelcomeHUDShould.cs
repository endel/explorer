using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using NSubstitute;
using NSubstitute.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tests
{
    public class WelcomeHUDControllerShould : TestsBase
    {
        [Test]
        public void CreateTheView()
        {
            // Arrange
            WelcomeHUDController controller = Substitute.ForPartsOf<WelcomeHUDController>();
            controller.Configure().CreateView().ReturnsForAnyArgs(Substitute.For<IWelcomeHUDView>());

            // Act
            controller.Initialize(null);

            //Assert
            controller.Received().CreateView();
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void ReactToViewOnButtonConfirm(int buttonIndexToPress)
        {
            // Arrange
            IWelcomeHUDView mockView = Substitute.For<IWelcomeHUDView>();
            mockView.When(x => x.Initialize(Arg.Any<UnityAction<int>>(), Arg.Any<UnityAction>(), Arg.Any<MessageOfTheDayConfig>()))
                .Do(x => x.ArgAt<UnityAction<int>>(0).Invoke(buttonIndexToPress));
            WelcomeHUDController controller = Substitute.ForPartsOf<WelcomeHUDController>();
            controller.Configure().CreateView().ReturnsForAnyArgs(mockView);

            // Act
            controller.Initialize(null);

            // Assert
            controller.Received().OnConfirmPressed(buttonIndexToPress);
            mockView.Received().SetVisible(false);
        }

        [Test]
        public void CallButtonAction()
        {
            // Arrange
            WelcomeHUDController controller = Substitute.ForPartsOf<WelcomeHUDController>();
            controller.Configure().CreateView().ReturnsForAnyArgs(Substitute.For<IWelcomeHUDView>());
            controller.Initialize( new MessageOfTheDayConfig
            {
                buttons = new []
                {
                    new MessageOfTheDayConfig.Button{action = "action0"},
                    new MessageOfTheDayConfig.Button{action = "action1"}
                }
            });

            // Act
            controller.OnConfirmPressed(1);

            // Assert
            controller.Received().SendAction("action1");
        }

        [Test]
        public void ProcessOutOfBoundsButtonsProperly()
        {
            // Arrange
            WelcomeHUDController controller = Substitute.ForPartsOf<WelcomeHUDController>();
            controller.Configure().CreateView().ReturnsForAnyArgs(Substitute.For<IWelcomeHUDView>());
            controller.Initialize( new MessageOfTheDayConfig { buttons = new MessageOfTheDayConfig.Button[0] });

            // Act
            controller.OnConfirmPressed(-1);
            controller.OnConfirmPressed(1);

            // Assert
            controller.DidNotReceiveWithAnyArgs().SendAction(default);
        }
    }

    public class WelcomeHUDViewShould : TestsBase
    {
        private WelcomeHUDView view;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            view = WelcomeHUDView.CreateView();
        }

        [Test]
        public void CreateViewProperly()
        {
            Assert.IsNotNull(view);
            Assert.IsNotNull(view.gameObject);
        }

        [Test]
        public void SetUICorrectly()
        {
            view.Initialize(null, null,
                new MessageOfTheDayConfig
                {
                    title = "title",
                    body = "body",
                });

            Assert.AreEqual("title", view.titleText.text);
            Assert.AreEqual("body", view.bodyText.text);
        }

        [Test]
        public void SetButtonsCorrectly()
        {
            view.Initialize(null, null,
                new MessageOfTheDayConfig
                {
                    buttons = new []
                    {
                        new MessageOfTheDayConfig.Button{ caption = "button0", tint = Color.green},
                        new MessageOfTheDayConfig.Button{ caption = "button1", tint = Color.blue},
                    }
                });

            Assert.AreEqual("button0", view.buttonsParent.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text);
            Assert.AreEqual(Color.green, view.buttonsParent.GetChild(0).GetComponentInChildren<Image>().color);
            Assert.AreEqual("button1", view.buttonsParent.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text);
            Assert.AreEqual(Color.blue, view.buttonsParent.GetChild(1).GetComponentInChildren<Image>().color);
        }

        protected override IEnumerator TearDown()
        {
            if(view != null && view.gameObject != null)
                Object.Destroy(view.gameObject);
            yield return base.TearDown();
        }
    }
}
