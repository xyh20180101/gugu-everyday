import { sha256 } from 'js-sha256'

export const getProgressText = (p) => {
  let text = '<未开始>'
  if (p.type?.progressType === 0) {
    if (p.extraData.totalProgress) {
      text = `${p.progresses[p.progresses.length - 1]?.currentProgress ?? '0'} / ${p.extraData.totalProgress}` + (p.extraData.progressUnit ? ` ${p.extraData.progressUnit}` : '')
    }
    else
      text = p.progresses[p.progresses.length - 1]?.currentProgress ?? text
  }
  else if (p.type?.progressType === 1) {
    text = p.progresses[p.progresses.length - 1]?.currentProgress ?? text
  }
  return text
}

export const solvePow = async (challenge, nonce) => {
  let counter = 0

  while (true) {
    for (let i = 0; i < 50000; i++) {
      const hash = sha256(nonce + counter)
      if (hash.includes(challenge)) {
        return { counter, hash }
      }
      counter++
    }
    await new Promise(r => setTimeout(r))
  }
}