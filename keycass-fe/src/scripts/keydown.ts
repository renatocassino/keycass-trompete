import { TextProcessor }from './textProcessor';

const textProcessor = new TextProcessor();

export function updateKeydown(event: KeyboardEvent): void {
	const currentLetter = event.key.toLowerCase();

    if (!currentLetter.match(/^[a-z ]$/)) {
        return
    }

    textProcessor.push(currentLetter);
}
