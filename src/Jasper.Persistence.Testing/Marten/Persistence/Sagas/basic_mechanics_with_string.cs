﻿using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.Marten.Persistence.Sagas
{
    public class StringBasicWorkflow : BasicWorkflow<StringWorkflowState, StringStart, StringCompleteThree, string>
    {
        public StringWorkflowState Starts(WildcardStart start)
        {
            var sagaId = start.Id;
            return new StringWorkflowState
            {
                Id = sagaId,
                Name = start.Name
            };
        }

        public void Handles(StringDoThree message, StringWorkflowState state)
        {
            state.ThreeCompleted = true;
        }
    }

    public class StringDoThree
    {
        [SagaIdentity] public string TheSagaId { get; set; }
    }

    public class basic_mechanics_with_String : SagaTestHarness<StringBasicWorkflow, StringWorkflowState>
    {
        private readonly string stateId = Guid.NewGuid().ToString();

        [Fact]
        public async Task complete()
        {
            await send(new StringStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            await send(new FinishItAll(), stateId);

            (await LoadState(stateId)).ShouldBeNull();
        }

        [Fact]
        public async Task handle_a_saga_message_with_cascading_messages_passes_along_the_saga_id_in_header()
        {
            await send(new StringStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            await send(new CompleteOne(), stateId);

            var state = await LoadState(stateId);
            state.OneCompleted.ShouldBeTrue();
            state.TwoCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task start_1()
        {
            await send(new StringStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            var state = await LoadState(stateId);

            state.ShouldNotBeNull();
            state.Name.ShouldBe("Croaker");
        }

        [Fact]
        public async Task start_2()
        {
            var code = codeFor<WildcardStart>();
            //throw new Exception(code);

            await send(new WildcardStart
            {
                Id = stateId,
                Name = "One Eye"
            });

            var state = await LoadState(stateId);

            state.ShouldNotBeNull();
            state.Name.ShouldBe("One Eye");
        }

        [Fact]
        public async Task straight_up_update_with_the_saga_id_on_the_message()
        {
            await send(new StringStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            var message = new StringCompleteThree
            {
                SagaId = stateId
            };

            await send(message);

            var state = await LoadState(stateId);
            state.ThreeCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task update_expecting_the_saga_id_to_be_on_the_envelope()
        {
            await send(new StringStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            await send(new CompleteFour(), stateId);

            var state = await LoadState(stateId);
            state.FourCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task update_with_message_that_uses_saga_identity_attributed_property()
        {
            await send(new StringStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            var message = new StringDoThree
            {
                TheSagaId = stateId
            };

            await send(message);

            var state = await LoadState(stateId);
            state.ThreeCompleted.ShouldBeTrue();
        }
    }
}
