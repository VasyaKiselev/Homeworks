def creat(file_obj):
    print("Введите заметки. Для завершения введите пустую строку.")
    
    note_count = 0
    while True:
        note = input(f"Заметка {note_count + 1}: ")
        
        if note == "":
            break
            
        file_obj.write(note + "\n")
        note_count += 1
        print(f"Заметка {note_count} сохранена!")
    
    print(f"\nВсего сохранено заметок: {note_count}")

def delete_all(file_obj):
    file_obj.seek(0)
    file_obj.truncate()
    print("Все заметки удалены")

def search1(file_obj):
    file_obj.seek(0)
    notes = file_obj.readlines()
    
    if not notes:
        print("Файл пуст")
        return
    
    text = input("Введите текст для поиска: ")
    found = []
    
    for i, note in enumerate(notes, 1):
        if text.lower() in note.lower():
            found.append((i, note.strip()))
    
    if found:
        print(f"Найдено: {len(found)}")
        for num, note in found:
            print(f"{num}. {note}")
    else:
        print("Не найдено")

def show(file_obj):
    file_obj.seek(0)
    notes = file_obj.readlines()
    
    if not notes:
        print("Файл пуст")
        return
    
    print("Все заметки:")
    for i, note in enumerate(notes, 1):
        print(f"{i}. {note.strip()}")

def close(f):
    f.close()


def show(f):
    with open(f, 'r', encoding='utf-8') as file:
        notes = file.readlines()
    
    if not notes:
        print("Файл пуст")
        return
    
    print("Все заметки:")
    for i, note in enumerate(notes, 1):
        print(f"{i}. {note.strip()}")


def exit():
    print("Closing programm...")
def interface():
    print("Hello, dear user! A'm manager of your notes.")
    while True:
        print('''There are commands you can use:
              1 - creat note
              2 - delete note
              3 - search for note
              4 - close note
              5 - show all notes
              6 - close programm
              Please enter the number of command you want to use: ''')
        answer = input()
        match answer:
            case "1":
               creat() 
            case "2":
               delete() 
            case "3":
               search() 
            case "4":
               close() 
            case "5":
               show() 
            case "6":
                exit()
                break
            case _:
                print("Please check your input. I don't thing there is a command with this number.")
                continue

file = open("Notes.txt", "r+")
interface()
